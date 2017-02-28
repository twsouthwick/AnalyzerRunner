using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

using static System.Console;

namespace AnalyzerRunner
{
    internal class ConsoleDiagnosticVisitor : DiagnosticVisitor
    {
        public override void AnalyzersFound(ImmutableArray<DiagnosticAnalyzer> analyzers)
        {
            if (analyzers.IsEmpty)
            {
                WriteLine("No analyzers found.");
            }
            else
            {
                WriteLine($"Found {analyzers.Length} analyzers.");
            }
        }

        public override void ProjectDiagnostics(Project project, ImmutableArray<Diagnostic> diagnostics)
        {
            if (diagnostics.IsEmpty)
            {
                WriteLine($"No diagnostics found for {project.Name}");
                return;
            }

            WriteLine($"Found {diagnostics.Length} diagnostics for {project.Name}");
            WriteLine();

            var table = diagnostics
                .Where(d => !string.Equals(d.Id, "AD0001", StringComparison.Ordinal))
                .ToStringTable(
                    new[] { "ID", "Severity", "Message", "Location" },
                    t => t.Id, t => t.Severity, t => t.GetMessage(), t => t.Location);

            WriteLine(table);
        }
    }
}