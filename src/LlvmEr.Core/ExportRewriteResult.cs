// Copyright (c) 2011-2026 Denis Kudelin
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.

namespace Itexoft.LlvmEr;

public sealed class ExportRewriteResult(
    string outputText,
    IReadOnlyList<string> matchedSymbols,
    IReadOnlyList<string> missingSymbols,
    int rewrittenLineCount)
{
    public string OutputText { get; } = outputText ?? throw new ArgumentNullException(nameof(outputText));
    public IReadOnlyList<string> MatchedSymbols { get; } = matchedSymbols ?? throw new ArgumentNullException(nameof(matchedSymbols));
    public IReadOnlyList<string> MissingSymbols { get; } = missingSymbols ?? throw new ArgumentNullException(nameof(missingSymbols));
    public int RewrittenLineCount { get; } = rewrittenLineCount;
    public bool HasChanges => this.RewrittenLineCount > 0;
}
