# Analyzer Runner

This repo contains a tool to run Roslyn based analyzers. Please file any issues that you find or feature requests. Currently, the tool can do the following:

- Accept any analyzer assembly
- Add additional files to compilation
- Enable features

## Usage

Basic usage can be seen by calling the tool with `-h`:

```
> .\AnalyzerRunner.exe -h
usage: AnalyzerRunner [-a <arg>...] [-d <arg>...] [-f <arg>...] [--] <path>

    -a, --analyzer <arg>...          Analyzer assembly to include
    -d, --additionalFile <arg>...    Additional files for compilation
    -f, --feature <arg>...           Experimental features for compilation.
    <path>                           Path to project or solution
```
