// Copyright (c) 2011-2026 Denis Kudelin
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.

namespace Itexoft.LlvmEr;

internal static class LlvmIrSymbolParser
{
    public static bool TryGetDefinedSymbol(string line, out string symbol)
    {
        symbol = string.Empty;

        if (string.IsNullOrWhiteSpace(line))
            return false;

        var trimmed = line.TrimStart();

        if (!IsDefineLine(trimmed))
            return false;

        var atIndex = trimmed.IndexOf('@');

        if (atIndex < 0 || atIndex + 1 >= trimmed.Length)
            return false;

        return TryReadSymbol(trimmed.AsSpan(atIndex), out symbol);
    }

    public static bool TryExtractSymbolFromToken(string token, out string symbol)
    {
        symbol = string.Empty;

        if (string.IsNullOrEmpty(token))
            return false;

        var atIndex = token.IndexOf('@');

        if (atIndex < 0 || atIndex + 1 >= token.Length)
            return false;

        return TryReadSymbol(token.AsSpan(atIndex), out symbol);
    }

    private static bool IsDefineLine(string trimmed)
    {
        if (!trimmed.StartsWith("define", StringComparison.Ordinal))
            return false;

        if (trimmed.Length == "define".Length)
            return false;

        return char.IsWhiteSpace(trimmed["define".Length]);
    }

    private static bool TryReadSymbol(ReadOnlySpan<char> span, out string symbol)
    {
        symbol = string.Empty;

        if (span.IsEmpty || span[0] != '@')
            return false;

        if (span.Length == 1)
            return false;

        if (span[1] == '"')
        {
            var endQuote = span.Slice(2).IndexOf('"');

            if (endQuote < 0)
                return false;

            symbol = new string(span.Slice(2, endQuote));

            return symbol.Length > 0;
        }

        var length = 0;

        for (var i = 1; i < span.Length; i++)
        {
            if (!IsSymbolChar(span[i]))
                break;

            length++;
        }

        if (length == 0)
            return false;

        symbol = new string(span.Slice(1, length));

        return true;
    }

    private static bool IsSymbolChar(char value) => char.IsLetterOrDigit(value) || value == '_' || value == '.' || value == '$';
}
