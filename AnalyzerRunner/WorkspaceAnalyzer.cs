using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnalyzerRunner
{
    internal class WorkspaceAnalyzer
    {
        private readonly DiagnosticVisitor _log;

        public WorkspaceAnalyzer(DiagnosticVisitor log)
        {
            _log = log;
        }

        public async Task AnalyzeAsync(Workspace ws, ImmutableArray<DiagnosticAnalyzer> analyzers, Options options, CancellationToken token)
        {
            _log.AnalyzersFound(analyzers);

            if (analyzers.IsEmpty)
            {
                return;
            }

            var sln = ws.CurrentSolution;

            foreach (var originalProject in sln.Projects)
            {
                var project = originalProject;

                foreach (var file in options.AdditionalFiles)
                {
                    project = project.AddAdditionalDocument(Path.GetFileName(file), file).Project;
                }

                project = project.WithParseOptions(project.ParseOptions.WithFeatures(options.Features));

                var additionalFiles = GenerateAdditionalFiles(options.AdditionalFiles);
                var compilation = await project.GetCompilationAsync(token).ConfigureAwait(false);
                var diagnostics = await compilation.WithAnalyzers(analyzers, new AnalyzerOptions(additionalFiles)).GetAllDiagnosticsAsync().ConfigureAwait(false);

                _log.ProjectDiagnostics(originalProject, diagnostics);
            }
        }

        private ImmutableArray<AdditionalText> GenerateAdditionalFiles(IReadOnlyList<string> additionalFiles)
        {
            return additionalFiles
                .Select(f => new PathAdditionalText(f))
                .Cast<AdditionalText>()
                .ToImmutableArray();
        }

        private class PathAdditionalText : AdditionalText
        {
            private readonly Lazy<SourceText> _text;

            public PathAdditionalText(string path)
            {
                Path = path;

                _text = new Lazy<SourceText>(() => SourceText.From(File.ReadAllText(Path)));
            }

            public override string Path { get; }

            public override SourceText GetText(CancellationToken cancellationToken = default(CancellationToken))
            {
                return _text.Value;
            }
        }
    }
}