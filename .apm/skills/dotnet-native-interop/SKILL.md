---
name: dotnet-native-interop
category: developer-experience
subcategory: cli
description: Calls native libraries via P/Invoke. LibraryImport, marshalling, cross-platform resolution.
license: MIT
targets: ['*']
tags: [csharp, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for csharp tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-native-interop

Platform Invoke (P/Invoke) patterns for calling native C/C++ libraries from .NET: `[LibraryImport]` (preferred, .NET 7+)
vs `[DllImport]` (legacy), struct marshalling, string marshalling, function pointer callbacks,
`NativeLibrary.SetDllImportResolver` for cross-platform library resolution, and platform-specific considerations for
Windows, macOS, Linux, iOS, and Android.

**Version assumptions:** .NET 7.0+ baseline for `[LibraryImport]`. `[DllImport]` available in all .NET versions.
`NativeLibrary` API available since .NET Core 3.0.

## Scope

- LibraryImport (.NET 7+) and DllImport declarations
- Struct and string marshalling patterns
- Function pointer callbacks and delegates
- NativeLibrary.SetDllImportResolver for cross-platform resolution

## Out of scope

- AOT-specific P/Invoke concerns (direct pinvoke) -- see [skill:dotnet-native-aot]
- COM interop and CsWin32 source generator -- see [skill:dotnet-winui]
- WASM JavaScript interop (JSImport/JSExport) -- see [skill:dotnet-aot-wasm]

Cross-references: [skill:dotnet-native-aot] for AOT-specific P/Invoke and `[LibraryImport]` in publish scenarios,
[skill:dotnet-aot-architecture] for AOT-first design patterns including source-generated interop, [skill:dotnet-winui]
for CsWin32 source generator and COM interop, [skill:dotnet-aot-wasm] for WASM JavaScript interop (not native P/Invoke).

---

## LibraryImport vs DllImport

`[LibraryImport]` (.NET 7+) is the preferred attribute for new P/Invoke declarations. It uses source generation to
produce marshalling code at compile time, making it fully AOT-compatible and eliminating runtime codegen overhead.

`[DllImport]` is the legacy attribute. It relies on runtime marshalling, which may require codegen not available in AOT
scenarios. Use `[DllImport]` only when targeting .NET 6 or earlier, or when the SYSLIB1054 analyzer indicates
`[LibraryImport]` cannot handle a specific signature.

### Decision Guide

| Scenario                                  | Use                                              |
| ----------------------------------------- | ------------------------------------------------ |
| New code targeting .NET 7+                | `[LibraryImport]`                                |
| Targeting .NET 6 or earlier               | `[DllImport]`                                    |
| SYSLIB1054 analyzer flags incompatibility | `[DllImport]` (with comment explaining why)      |
| Publishing with Native AOT                | `[LibraryImport]` (required for full AOT compat) |

### LibraryImport Declaration

````csharp

using System.Runtime.InteropServices;

public static partial class NativeApi
{
    [LibraryImport("mylib")]
    internal static partial int ProcessData(
        ReadOnlySpan<byte> input,
        int length);

    [LibraryImport("mylib", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int OpenByName(string name);

    [LibraryImport("mylib", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseResource(nint handle);
}

```text

Key requirements for `[LibraryImport]`:
- Method must be `static partial` in a `partial` class
- String marshalling must be explicitly specified via `StringMarshalling` or `[MarshalAs]` on each string parameter (only needed when strings are present)
- Boolean return types require explicit `[return: MarshalAs(UnmanagedType.Bool)]`
- `Span<T>` and `ReadOnlySpan<T>` parameters are supported directly -- `[DllImport]` does not support them (use arrays instead)

### DllImport Declaration (Legacy)

```csharp

using System.Runtime.InteropServices;

public static class NativeApiLegacy
{
    [DllImport("mylib", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int ProcessData(
        byte[] input,
        int length);

    [DllImport("mylib", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CloseResource(IntPtr handle);
}

```text

### Migrating DllImport to LibraryImport

The `SYSLIB1054` analyzer suggests converting `[DllImport]` to `[LibraryImport]` and provides code fixes. Key changes:

1. Replace `[DllImport]` with `[LibraryImport]`
2. Change `static extern` to `static partial`
3. Make the containing class `partial`
4. Replace `CharSet` with `StringMarshalling`
5. Replace `IntPtr` with `nint` where appropriate
6. Add explicit `[MarshalAs]` for `bool` parameters and returns

```csharp

// Before (DllImport)
[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
static extern IntPtr LoadLibrary(string lpLibFileName);

// After (LibraryImport)
[LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16,
    SetLastError = true)]
internal static partial nint LoadLibrary(string lpLibFileName);

```text

---

## Platform-Specific Library Names

Native library names differ across platforms. Use `NativeLibrary.SetDllImportResolver` or conditional compilation to handle this.

### Windows

Windows uses `.dll` files. The loader searches the application directory, system directories, and `PATH`.

```csharp

// Windows library name includes .dll extension
[LibraryImport("sqlite3.dll")]
internal static partial int sqlite3_open(
    [MarshalAs(UnmanagedType.LPUTF8Str)] string filename,
    out nint db);

```text

Windows also supports omitting the extension -- the loader appends `.dll` automatically:

```csharp

[LibraryImport("sqlite3")]
internal static partial int sqlite3_open(
    [MarshalAs(UnmanagedType.LPUTF8Str)] string filename,
    out nint db);

```text

### macOS and Linux

macOS uses `.dylib` files; Linux uses `.so` files. The .NET runtime automatically probes common name variations (with and without `lib` prefix, with platform-specific extensions).

```csharp

// Use the logical name without extension -- .NET probes:
// libsqlite3.dylib (macOS), libsqlite3.so (Linux), sqlite3.dll (Windows)
[LibraryImport("libsqlite3")]
internal static partial int sqlite3_open(
    [MarshalAs(UnmanagedType.LPUTF8Str)] string filename,
    out nint db);

```text

.NET probing order for library name `"foo"`:
1. `foo` (exact name)
2. `foo.dll`, `foo.so`, `foo.dylib` (platform extension)
3. `libfoo`, `libfoo.so`, `libfoo.dylib` (lib prefix + extension)

### iOS

iOS does not allow loading dynamic libraries at runtime. Native code must be statically linked into the application binary. Use `__Internal` as the library name to call functions linked into the main executable:

```csharp

// Calls a function statically linked into the iOS app binary
[LibraryImport("__Internal")]
internal static partial int NativeFunction(int input);

```csharp

For iOS, the native library must be compiled as a static library (`.a`) and linked during the Xcode build phase. MAUI and Xamarin handle this through native references in the project file:

```xml

<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">
  <NativeReference Include="libs/libmynative.a">
    <Kind>Static</Kind>
    <ForceLoad>true</ForceLoad>
  </NativeReference>
</ItemGroup>

```text

### Android

Android uses `.so` files loaded from the app's native library directory. The library name typically omits the `lib` prefix and `.so` extension in the P/Invoke declaration:

```csharp

// Android loads libmynative.so from the APK's lib/<abi>/ directory
[LibraryImport("mynative")]
internal static partial int NativeFunction(int input);

```csharp

Include platform-specific `.so` files for each target ABI in the project:

```xml

<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
  <AndroidNativeLibrary Include="libs/arm64-v8a/libmynative.so" Abi="arm64-v8a" />
  <AndroidNativeLibrary Include="libs/x86_64/libmynative.so" Abi="x86_64" />
</ItemGroup>

```text

### WASM

WebAssembly does not support traditional P/Invoke. Native C/C++ code cannot be called via `[LibraryImport]` or `[DllImport]` in browser WASM. For JavaScript interop, see [skill:dotnet-aot-wasm].

---

## NativeLibrary.SetDllImportResolver

`NativeLibrary.SetDllImportResolver` (.NET Core 3.0+) provides runtime control over library resolution. This is the recommended approach for cross-platform library loading when static name probing is insufficient.

```csharp

using System.Reflection;
using System.Runtime.InteropServices;

// Register once at startup (per assembly)
NativeLibrary.SetDllImportResolver(
    Assembly.GetExecutingAssembly(),
    DllImportResolver);

static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
{
    if (libraryName == "mynativelib")
    {
        if (OperatingSystem.IsWindows())
            return NativeLibrary.Load("mynative.dll", assembly, searchPath);

        if (OperatingSystem.IsMacOS())
            return NativeLibrary.Load("libmynative.dylib", assembly, searchPath);

        if (OperatingSystem.IsLinux())
            return NativeLibrary.Load("libmynative.so.1", assembly, searchPath);
    }

    // Fall back to default resolution
    return nint.Zero;
}

```text

### Common Use Cases for DllImportResolver

| Scenario | Why resolver is needed |
|----------|----------------------|
| Versioned `.so` on Linux (e.g., `libfoo.so.2`) | Default probing does not check versioned names |
| Library in a non-standard path | Load from a custom directory at runtime |
| Bundled native library per RID | Resolve to `runtimes/<rid>/native/` path |
| Feature detection at load time | Try multiple library names and fall back gracefully |

### NativeLibrary API

The `NativeLibrary` class provides low-level library management:

```csharp

// Load a library explicitly
nint handle = NativeLibrary.Load("mylib");

// Try to load without throwing
if (NativeLibrary.TryLoad("mylib", out nint h))
{
    // Get a function pointer by name
    nint funcPtr = NativeLibrary.GetExport(h, "my_function");

    // Or try without throwing
    if (NativeLibrary.TryGetExport(h, "my_function", out nint fp))
    {
        // Use function pointer
    }

    NativeLibrary.Free(h);
}

```text

---

## Marshalling Patterns

### Struct Marshalling

Structs passed to native code must have a well-defined memory layout. Use `[StructLayout]` to control layout and alignment.

```csharp

using System.Runtime.InteropServices;

// Sequential layout -- fields laid out in declaration order
[StructLayout(LayoutKind.Sequential)]
public struct Point
{
    public int X;
    public int Y;
}

// Explicit layout -- fields at specific byte offsets (for unions)
[StructLayout(LayoutKind.Explicit)]
public struct ValueUnion
{
    [FieldOffset(0)] public int IntValue;
    [FieldOffset(0)] public float FloatValue;
    [FieldOffset(0)] public double DoubleValue;
}

// Sequential with packing -- override default alignment
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PackedHeader
{
    public byte Magic;
    public int Length;     // No padding before this field
    public short Version;
}

```text

**Blittable structs** (containing only primitive value types with sequential/explicit layout) are passed directly to native code without copying. Non-blittable structs require marshalling, which incurs overhead.

Blittable primitive types: `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `nint`, `nuint`.

**Not blittable:** `bool` (marshals as 4-byte `BOOL` by default), `char` (depends on charset), `string`, arrays of non-blittable types.

### String Marshalling

Specify string encoding explicitly. Never rely on default marshalling behavior.

```csharp

// UTF-8 strings (most common for cross-platform C APIs)
[LibraryImport("mylib", StringMarshalling = StringMarshalling.Utf8)]
internal static partial int ProcessText(string input);

// UTF-16 strings (Windows APIs)
[LibraryImport("mylib", StringMarshalling = StringMarshalling.Utf16)]
internal static partial int ProcessTextW(string input);

// Per-parameter marshalling when methods mix encodings
[LibraryImport("mylib")]
internal static partial int MixedApi(
    [MarshalAs(UnmanagedType.LPUTF8Str)] string utf8Param,
    [MarshalAs(UnmanagedType.LPWStr)] string utf16Param);

```text

For output string buffers, use `char[]` or `byte[]` from `ArrayPool` instead of `StringBuilder`:

```csharp

[LibraryImport("mylib")]
internal static partial int GetName(
    [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] char[] buffer,
    int bufferSize);

// Usage
char[] buffer = ArrayPool<char>.Shared.Rent(256);
try
{
    int result = GetName(buffer, buffer.Length);
    string name = new string(buffer, 0, result);
}

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
