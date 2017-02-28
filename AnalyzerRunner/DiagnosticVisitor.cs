using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace AnalyzerRunner
{
    public abstract class DiagnosticVisitor
    {
        public virtual void ProjectDiagnostics(Project originalProject, ImmutableArray<Diagnostic> diagnostics)
        {
        }

        public virtual void AnalyzersFound(ImmutableArray<DiagnosticAnalyzer> analyzers)
        {
        }
    }
}