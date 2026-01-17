// Copyright (c) 2011-2026 Denis Kudelin
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.

namespace Itexoft.LlvmEr;

public sealed class ExportRewriteInput(string sourceText, IReadOnlyCollection<string> exports)
{
    public string SourceText { get; } = sourceText ?? throw new ArgumentNullException(nameof(sourceText));
    public IReadOnlyCollection<string> Exports { get; } = exports ?? throw new ArgumentNullException(nameof(exports));
}
