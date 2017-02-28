using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AnalyzerRunner
{
    internal class WorkspaceLoader
    {
        private static readonly Type[] s_codeAnalysisTypes = new[]
        {
            typeof(Microsoft.CodeAnalysis.CSharp.SyntaxKind),
            typeof(Microsoft.CodeAnalysis.VisualBasic.SyntaxKind)
        };

        public static ImmutableArray<DiagnosticAnalyzer> GetDiagnosticAnalyzers(IReadOnlyCollection<string> analyzers)
        {
            var analyzerAssemblies = analyzers
                .Select(a => Assembly.LoadFrom(a))
                .ToList();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .ToDictionary(a => a.GetName().Name, StringComparer.OrdinalIgnoreCase);
            var warnings = 0;

            Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
            {
                var name = new AssemblyName(args.Name);
                if (assemblies.TryGetValue(name.Name, out var assembly))
                {
                    var version = assembly.GetName().Version;

                    if (name.Version != version)
                    {
                        Console.WriteLine($"Warning: Expected {name.Version} but got {version} for {name.Name}");
                        warnings++;
                    }

                    return assembly;
                }

                return null;
            }

            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                return analyzerAssemblies
                    .SelectMany(a => a.DefinedTypes.Where(t => !t.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(t)))
                    .Select(Activator.CreateInstance)
                    .Cast<DiagnosticAnalyzer>()
                    .ToImmutableArray();
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

                if (warnings > 0)
                {
                    Console.WriteLine();
                }
            }
        }

        public static async Task<Workspace> LoadWorkspaceAsync(string path, CancellationToken token)
        {
            var ws = MSBuildWorkspace.Create();

            if (path.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            {
                await ws.OpenSolutionAsync(path, token).ConfigureAwait(false);
            }
            else
            {
                await ws.OpenProjectAsync(path, token).ConfigureAwait(false);
            }

            return ws;
        }
    }
}