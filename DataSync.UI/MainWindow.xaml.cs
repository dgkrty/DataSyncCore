using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using DataSync.Core.Models;
using DataSync.Core.Resolution;

namespace DataSync.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Log("System Initialized. Ready to connect to source database.");
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            BtnGenerate.IsEnabled = false;
            TxtConsole.Text = string.Empty;
            Log("Connecting to source database...");
            await Task.Delay(500); // Simulate network delay

            Log("Introspecting schema graph...");
            var tables = GetMockSchema();
            await Task.Delay(500);
            Log($"Found {tables.Count} tables. Resolving topological dependencies...\n");

            // Execute the Core Engine logic
            var resolver = new TopologicalDependencyResolver();
            var plan = resolver.ResolveExecutionOrder(tables);

            Log("--- COMPUTED HYDRATION ORDER ---");
            for (int i = 0; i < plan.OrderedTables.Count; i++)
            {
                var t = plan.OrderedTables[i];
                Log($"[Phase {i + 1}] {t.FullName} (~{t.EstimatedRowCount:N0} rows)");
                await Task.Delay(200); // Simulate processing time for visual effect
            }

            Log("\n--- CYCLE-BREAKER REPAIRS ---");
            if (plan.CycleBreakerRepairs.Count == 0) Log("None detected.");
            foreach (var repair in plan.CycleBreakerRepairs)
            {
                Log($"[DEFER] {repair.ConstraintName} on {repair.SourceTable}.{repair.SourceColumn} -> {repair.TargetTable}.{repair.TargetColumn}");
            }

            Log("\nSUCCESS: Offline package generated at C:\\DataSync\\Exports\\");
            BtnGenerate.IsEnabled = true;
        }

        private void Log(string message)
        {
            TxtConsole.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            // Scroll to bottom simulation
            TxtConsole.BringIntoView();
        }

        // Simulates the schema we built in the CLI tester
        private List<TableSchema> GetMockSchema()
        {
            return new List<TableSchema>
            {
                new("dbo", "Departments", 25, new List<ColumnSchema>(), new List<ForeignKeyEdge>
                {
                    new("FK_Dept_Manager", "dbo", "Departments", "ManagerEmployeeId", "dbo", "Employees", "Id", true)
                }),
                new("dbo", "Employees", 500, new List<ColumnSchema>(), new List<ForeignKeyEdge>
                {
                    new("FK_Emp_Dept", "dbo", "Employees", "DepartmentId", "dbo", "Departments", "Id", false)
                }),
                new("dbo", "AuditLogs", 12000, new List<ColumnSchema>(), new List<ForeignKeyEdge>
                {
                    new("FK_Audit_Emp", "dbo", "AuditLogs", "EmployeeId", "dbo", "Employees", "Id", false)
                })
            };
        }
    }
}
