# llvm-er

`llvm-er` is a small export-rewriter for LLVM IR. It externalizes function definitions listed in an export file, so
linkers can actually export those symbols.

This is useful for building importable wasm side modules from .NET NativeAOT, where `UnmanagedCallersOnly` exports can
otherwise end up with local linkage.

## Usage

```bash
llvm-er --exports exports.txt -o out.ll input.ll
llvm-er --exports exports.txt --inplace input.ll
llvm-er --exports exports.txt -o - input.ll
```

### Export list format

- One symbol per line.
- Lines starting with `#`, `//`, or `;` are ignored.
- A leading `@` is allowed and ignored.

## Build

```bash
dotnet publish -c Release -r <rid> src/LlvmEr/LlvmEr.csproj
```

## Package layout

Native binaries are packed into:

```
runtimes/<rid>/native/llvm-er[.exe]
```

## License

MPL-2.0-no-copyleft-exception. See `LICENSE.md`.
