// Copyright (c) 2011-2026 Denis Kudelin
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.

using Itexoft.LlvmEr.Properties;

namespace Itexoft.LlvmEr;

internal static class Program
{
    private const string toolName = "llvm-er";

    public static int Main(string[] args)
    {
        var options = new Options();

        if (!TryParseArguments(args, options, out var error, out var showHelp, out var showVersion))
        {
            if (!string.IsNullOrEmpty(error))
                WriteError(error);

            PrintUsage(Console.Error);

            return ExitCodes.Usage;
        }

        if (showHelp)
        {
            PrintUsage(Console.Out);

            return ExitCodes.Success;
        }

        if (showVersion)
        {
            PrintVersion(Console.Out);

            return ExitCodes.Success;
        }

        try
        {
            var exports = ExportSymbolListReader.Read(options.ExportsPath!);

            if (exports.Count == 0)
            {
                WriteError("Export list is empty.");

                return ExitCodes.ProcessingError;
            }

            var inputText = File.ReadAllText(options.InputPath!);
            var rewriter = new ExportRewriter();
            var result = rewriter.Rewrite(inputText, exports);

            if (result.MissingSymbols.Count > 0)
            {
                WriteError($"Missing exports in LLVM IR: {string.Join(", ", result.MissingSymbols)}");

                return ExitCodes.ProcessingError;
            }

            if (options.OutputToStdout)
            {
                Console.Out.Write(result.OutputText);

                return ExitCodes.Success;
            }

            var outputPath = options.InPlace ? options.InputPath! : options.OutputPath!;
            File.WriteAllText(outputPath, result.OutputText);

            return ExitCodes.Success;
        }
        catch (Exception ex)
        {
            WriteError(ex.Message);

            return ExitCodes.ProcessingError;
        }
    }

    private static bool TryParseArguments(string[] args, Options options, out string error, out bool showHelp, out bool showVersion)
    {
        error = string.Empty;
        showHelp = false;
        showVersion = false;

        if (args.Length == 0)
        {
            error = "No arguments provided.";

            return false;
        }

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg is "--help" or "-h" or "-?")
            {
                showHelp = true;

                return true;
            }

            if (arg is "--version")
            {
                showVersion = true;

                return true;
            }

            if (arg == "--exports")
            {
                if (!TryConsumeValue(args, ref i, out var value))
                {
                    error = "--exports requires a value.";

                    return false;
                }

                options.ExportsPath = value;

                continue;
            }

            if (arg == "-o")
            {
                if (!TryConsumeValue(args, ref i, out var value))
                {
                    error = "-o requires a value.";

                    return false;
                }

                options.OutputPath = value;

                continue;
            }

            if (arg == "--inplace")
            {
                options.InPlace = true;

                continue;
            }

            if (arg.StartsWith('-'))
            {
                error = $"Unknown option: {arg}";

                return false;
            }

            if (!string.IsNullOrEmpty(options.InputPath))
            {
                error = "Multiple input files are not supported.";

                return false;
            }

            options.InputPath = arg;
        }

        if (string.IsNullOrWhiteSpace(options.ExportsPath))
        {
            error = "--exports is required.";

            return false;
        }

        if (string.IsNullOrWhiteSpace(options.InputPath))
        {
            error = "Input file is required.";

            return false;
        }

        if (options.InPlace && !string.IsNullOrEmpty(options.OutputPath))
        {
            error = "--inplace and -o cannot be used together.";

            return false;
        }

        if (!options.InPlace && string.IsNullOrEmpty(options.OutputPath))
        {
            error = "-o or --inplace is required.";

            return false;
        }

        if (options.OutputPath == "-" && options.InPlace)
        {
            error = "--inplace cannot be used with -o -.";

            return false;
        }

        options.OutputToStdout = options.OutputPath == "-";

        return true;
    }

    private static bool TryConsumeValue(string[] args, ref int index, out string value)
    {
        if (index + 1 >= args.Length)
        {
            value = string.Empty;

            return false;
        }

        index++;
        value = args[index];

        return true;
    }

    private static void PrintUsage(TextWriter writer)
    {
        writer.WriteLine("llvm-er - export rewrite utility for LLVM IR");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  llvm-er --exports <file> -o <out> <input>");
        writer.WriteLine("  llvm-er --exports <file> --inplace <input>");
        writer.WriteLine("  llvm-er --exports <file> -o - <input>");
        writer.WriteLine();
        writer.WriteLine("Options:");
        writer.WriteLine("  --exports <file>   List of symbols to externalize (one per line). Required.");
        writer.WriteLine("  -o <path>          Output file path. Use '-' for stdout.");
        writer.WriteLine("  --inplace          Rewrite the input file in place.");
        writer.WriteLine("  --help, -h         Show this help.");
        writer.WriteLine("  --version          Show version.");
    }

    private static void PrintVersion(TextWriter writer) =>
        writer.WriteLine(AssemblyMetadata.ProductVersion is null ? toolName : $"{toolName} {AssemblyMetadata.ProductVersion}");

    private static void WriteError(string message) => Console.Error.WriteLine($"{toolName}: error: {message}");

    private sealed class Options
    {
        public string? InputPath { get; set; }
        public string? ExportsPath { get; set; }
        public string? OutputPath { get; set; }
        public bool InPlace { get; set; }
        public bool OutputToStdout { get; set; }
    }

    private static class ExitCodes
    {
        public const int Success = 0;
        public const int Usage = 1;
        public const int ProcessingError = 2;
    }
}
