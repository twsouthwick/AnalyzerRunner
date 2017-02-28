using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace AnalyzerRunner
{
    partial class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var options = Options.Parse(args);

                options.PrintOptions();

                RunAsync(options, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task RunAsync(Options options, CancellationToken token)
        {
            var analyzers = WorkspaceLoader.GetDiagnosticAnalyzers(options.Analyzers);
            var ws = await WorkspaceLoader.LoadWorkspaceAsync(options.Path, token).ConfigureAwait(false);

            var analyzer = new WorkspaceAnalyzer(new ConsoleDiagnosticVisitor());
            await analyzer.AnalyzeAsync(ws, analyzers, options, token).ConfigureAwait(false);
        }
    }
}