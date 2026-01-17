// Copyright (c) 2011-2026 Denis Kudelin
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.

namespace Itexoft.LlvmEr;

public static class ExportSymbolListReader
{
    public static IReadOnlyList<string> Read(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Export list path is required.", nameof(path));

        var lines = File.ReadAllLines(path);
        var symbols = new List<string>(lines.Length);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var line in lines)
        {
            var symbol = Normalize(line);

            if (string.IsNullOrEmpty(symbol))
                continue;

            if (seen.Add(symbol))
                symbols.Add(symbol);
        }

        return symbols;
    }

    private static string? Normalize(string line)
    {
        if (line is null)
            return null;

        var trimmed = line.Trim();

        if (trimmed.Length == 0)
            return null;

        if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith('#') || trimmed.StartsWith(';'))
            return null;

        if (trimmed[0] == '@')
            trimmed = trimmed[1..];

        return trimmed.Length == 0 ? null : trimmed;
    }
}
