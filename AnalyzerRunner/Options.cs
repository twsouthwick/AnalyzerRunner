using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;

namespace AnalyzerRunner
{
    internal struct Options
    {
        public string Path;
        public IReadOnlyList<string> Analyzers;
        public IReadOnlyList<string> AdditionalFiles;
        public IReadOnlyList<KeyValuePair<string, string>> Features;

        public static Options Parse(string[] args)
        {
            var options = new Options
            {
                Path = null,
                Analyzers = Array.Empty<string>(),
                AdditionalFiles = Array.Empty<string>(),
                Features = Array.Empty<KeyValuePair<string, string>>()
            };

            var r = ArgumentSyntax.Parse(args, syntax =>
            {
                KeyValuePair<string, string> ParseFeature(string feature)
                {
                    var split = feature.Split(':');

                    if (split.Length != 2)
                    {
                        syntax.ReportError($"Invalid feature '{feature}'. Must be of form '[name]:[value]'");
                    }

                    return new KeyValuePair<string, string>(split[0], split[1]);
                }

                syntax.DefineOptionList("a|analyzer", ref options.Analyzers, "Analyzer assembly to include");
                syntax.DefineOptionList("d|additionalFile", ref options.AdditionalFiles, "Additional files for compilation");
                syntax.DefineOptionList("f|feature", ref options.Features, ParseFeature, "Experimental features for compilation.");
                syntax.DefineParameter("path", ref options.Path, "Path to project or solution");
            });

            if (!File.Exists(options.Path))
            {
                Console.WriteLine("error: Project or solution path not supplied");

                // Using Environment.Exit because the argument parser uses it at the momemt
                Environment.Exit(0);
            }

            return options;
        }

        public void PrintOptions()
        {
            Console.WriteLine($"Project: {Path}");

            foreach (var analyzer in Analyzers)
            {
                Console.WriteLine($"Analyzer assembly: {analyzer}");
            }

            foreach (var additionalFile in AdditionalFiles)
            {
                Console.WriteLine($"Additional file: {additionalFile}");
            }

            foreach (var feature in Features)
            {
                Console.WriteLine($"Feature: {feature.Key} = {feature.Value}");
            }
        }
    }
}