using System;
using System.Collections.Generic;

namespace DataSync.Core.Models;

public record ColumnSchema(
    string Name,
    string DataType,
    int? MaxLength,
    bool IsNullable,
    bool IsPrimaryKey,
    bool IsIdentity
);

public record ForeignKeyEdge(
    string ConstraintName,
    string SourceSchema,
    string SourceTable,
    string SourceColumn,
    string TargetSchema,
    string TargetTable,
    string TargetColumn,
    bool IsSourceNullable
);

public record TableSchema(
    string Schema,
    string Name,
    long EstimatedRowCount,
    List<ColumnSchema> Columns,
    List<ForeignKeyEdge> OutboundForeignKeys
)
{
    public string FullName => $"{Schema}.{Name}";
}

public record DeferredForeignKeyRepair(
    string SourceTable,
    string SourceColumn,
    string TargetTable,
    string TargetColumn,
    string ConstraintName
);

public record ExecutionPlan(
    List<TableSchema> OrderedTables,
    List<DeferredForeignKeyRepair> CycleBreakerRepairs
);
