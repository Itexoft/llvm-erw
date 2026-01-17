// Copyright (c) 2011-2026 Denis Kudelin
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.

namespace Itexoft.LlvmEr;

public sealed class ExportRewriter : IExportRewriter
{
    private static readonly HashSet<string> linkageTokens = new(StringComparer.Ordinal)
    {
        "internal",
        "private",
        "linkonce",
        "linkonce_odr",
        "weak",
        "weak_odr",
        "available_externally",
    };

    private static readonly HashSet<string> visibilityTokens = new(StringComparer.Ordinal)
    {
        "hidden",
        "protected",
    };

    public ExportRewriteResult Rewrite(ExportRewriteInput input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return this.Rewrite(input.SourceText, input.Exports);
    }

    public ExportRewriteResult Rewrite(string inputText, IReadOnlyCollection<string> exports)
    {
        if (inputText is null)
            throw new ArgumentNullException(nameof(inputText));

        if (exports is null)
            throw new ArgumentNullException(nameof(exports));

        var newline = DetectNewLine(inputText);
        var trailingNewLine = inputText.EndsWith(newline, StringComparison.Ordinal);
        var lines = SplitLines(inputText, newline);

        var exportSet = new HashSet<string>(exports, StringComparer.Ordinal);
        var matched = new HashSet<string>(StringComparer.Ordinal);
        var rewrittenLineCount = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (!LlvmIrSymbolParser.TryGetDefinedSymbol(line, out var symbol))
                continue;

            if (!exportSet.Contains(symbol))
                continue;

            matched.Add(symbol);

            if (TryRewriteDefineLine(line, symbol, out var updated))
            {
                lines[i] = updated;
                rewrittenLineCount++;
            }
        }

        var missing = new List<string>();
        var matchedOrdered = new List<string>();

        foreach (var symbol in exports)
        {
            if (matched.Contains(symbol))
                matchedOrdered.Add(symbol);
            else
                missing.Add(symbol);
        }

        var output = string.Join(newline, lines);

        if (trailingNewLine)
            output += newline;

        return new ExportRewriteResult(output, matchedOrdered, missing, rewrittenLineCount);
    }

    private static bool TryRewriteDefineLine(string line, string symbol, out string updated)
    {
        updated = line;
        var trimmed = line.TrimStart();

        if (!trimmed.StartsWith("define", StringComparison.Ordinal))
            return false;

        var indentLength = line.Length - trimmed.Length;
        var indent = indentLength > 0 ? line[..indentLength] : string.Empty;

        var tokens = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
            return false;

        var symbolTokenIndex = FindSymbolTokenIndex(tokens, symbol);

        if (symbolTokenIndex < 0)
            return false;

        var rewritten = new List<string>(tokens.Length);
        var changed = false;

        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];

            if (i < symbolTokenIndex && ShouldRemoveToken(token))
            {
                changed = true;

                continue;
            }

            rewritten.Add(token);
        }

        if (!changed)
            return false;

        updated = indent + string.Join(" ", rewritten);

        return true;
    }

    private static int FindSymbolTokenIndex(string[] tokens, string symbol)
    {
        for (var i = 0; i < tokens.Length; i++)
        {
            if (!LlvmIrSymbolParser.TryExtractSymbolFromToken(tokens[i], out var tokenSymbol))
                continue;

            if (string.Equals(tokenSymbol, symbol, StringComparison.Ordinal))
                return i;
        }

        return -1;
    }

    private static bool ShouldRemoveToken(string token) => linkageTokens.Contains(token) || visibilityTokens.Contains(token);

    private static string DetectNewLine(string input) => input.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";

    private static string[] SplitLines(string input, string newline) => input.Split([newline], StringSplitOptions.None);
}
