using System;
using System.Collections.Generic;
using System.Linq;
using DataSync.Core.Models;

namespace DataSync.Core.Resolution;

public class TopologicalDependencyResolver
{
    public ExecutionPlan ResolveExecutionOrder(IReadOnlyList<TableSchema> tables)
    {
        var tableMap = tables.ToDictionary(t => t.FullName, StringComparer.OrdinalIgnoreCase);
        var inDegree = tables.ToDictionary(t => t.FullName, _ => 0, StringComparer.OrdinalIgnoreCase);
        var dependents = tables.ToDictionary(t => t.FullName, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
        var deferredRepairs = new List<DeferredForeignKeyRepair>();

        foreach (var table in tables)
        {
            foreach (var fk in table.OutboundForeignKeys)
            {
                var parentKey = $"{fk.TargetSchema}.{fk.TargetTable}";
                var childKey = table.FullName;

                if (string.Equals(parentKey, childKey, StringComparison.OrdinalIgnoreCase))
                {
                    deferredRepairs.Add(new DeferredForeignKeyRepair(
                        childKey, fk.SourceColumn, parentKey, fk.TargetColumn, fk.ConstraintName));
                    continue;
                }

                if (tableMap.ContainsKey(parentKey) && dependents[parentKey].Add(childKey))
                {
                    inDegree[childKey]++;
                }
            }
        }

        var readyQueue = new Queue<string>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        var orderedTables = new List<TableSchema>(tables.Count);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (orderedTables.Count < tables.Count)
        {
            while (readyQueue.Count > 0)
            {
                var currentKey = readyQueue.Dequeue();
                if (!visited.Add(currentKey)) continue;

                orderedTables.Add(tableMap[currentKey]);

                foreach (var childKey in dependents[currentKey])
                {
                    inDegree[childKey]--;
                    if (inDegree[childKey] == 0) readyQueue.Enqueue(childKey);
                }
            }

            if (orderedTables.Count < tables.Count)
            {
                var remainingNodes = inDegree.Keys.Where(k => !visited.Contains(k)).ToList();
                var candidateNode = remainingNodes
                    .OrderByDescending(k => tableMap[k].OutboundForeignKeys.Any(fk => 
                        fk.IsSourceNullable && !visited.Contains($"{fk.TargetSchema}.{fk.TargetTable}")))
                    .ThenBy(k => inDegree[k])
                    .First();

                foreach (var fk in tableMap[candidateNode].OutboundForeignKeys)
                {
                    var parentKey = $"{fk.TargetSchema}.{fk.TargetTable}";
                    if (!visited.Contains(parentKey))
                    {
                        deferredRepairs.Add(new DeferredForeignKeyRepair(
                            candidateNode, fk.SourceColumn, parentKey, fk.TargetColumn, fk.ConstraintName));
                    }
                }

                inDegree[candidateNode] = 0;
                readyQueue.Enqueue(candidateNode);
            }
        }

        return new ExecutionPlan(orderedTables, deferredRepairs);
    }
}
