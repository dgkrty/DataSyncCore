using System;
using System.Collections.Generic;
using DataSync.Core.Models;
using DataSync.Core.Resolution;

Console.WriteLine("=== DataSync Core: Offline DAG & Cycle Resolver Test ===\n");

// Simulate two tables with a circular Foreign Key dependency
var tables = new List<TableSchema>
{
    new("dbo", "Departments", 25,
        new List<ColumnSchema>
        {
            new("Id", "int", 4, false, true, true),
            new("ManagerEmployeeId", "int", 4, true, false, false)
        },
        new List<ForeignKeyEdge>
        {
            new("FK_Dept_Manager", "dbo", "Departments", "ManagerEmployeeId", "dbo", "Employees", "Id", IsSourceNullable: true)
        }),

    new("dbo", "Employees", 500,
        new List<ColumnSchema>
        {
            new("Id", "int", 4, false, true, true),
            new("DepartmentId", "int", 4, false, false, false)
        },
        new List<ForeignKeyEdge>
        {
            new("FK_Emp_Dept", "dbo", "Employees", "DepartmentId", "dbo", "Departments", "Id", IsSourceNullable: false)
        }),

    new("dbo", "AuditLogs", 12000,
        new List<ColumnSchema> { new("Id", "bigint", 8, false, true, true) },
        new List<ForeignKeyEdge>
        {
            new("FK_Audit_Emp", "dbo", "AuditLogs", "EmployeeId", "dbo", "Employees", "Id", IsSourceNullable: false)
        })
};

var resolver = new TopologicalDependencyResolver();
var plan = resolver.ResolveExecutionOrder(tables);

Console.WriteLine("1. Computed Topological Hydration Order:");
for (int i = 0; i < plan.OrderedTables.Count; i++)
{
    var t = plan.OrderedTables[i];
    Console.WriteLine($"   Phase {i + 1}: {t.FullName} (~{t.EstimatedRowCount:N0} rows)");
}

Console.WriteLine("\n2. Cycle-Breaker Deferred Constraints:");
foreach (var repair in plan.CycleBreakerRepairs)
{
    Console.WriteLine($"   [DEFER] {repair.ConstraintName} on {repair.SourceTable}.{repair.SourceColumn} -> {repair.TargetTable}.{repair.TargetColumn}");
}