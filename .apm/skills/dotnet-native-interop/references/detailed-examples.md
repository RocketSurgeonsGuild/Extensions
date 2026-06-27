}
finally
{
    ArrayPool<char>.Shared.Return(buffer);
}

```text

### Function Pointer Callbacks

Modern .NET (.NET 5+) prefers unmanaged function pointers over delegate-based callbacks for better performance and AOT compatibility.

**Preferred: Unmanaged function pointers with `[UnmanagedCallersOnly]`**

```csharp

using System.Runtime.InteropServices;

// Native callback signature: int (*callback)(int value, void* context)
[LibraryImport("mylib")]
internal static unsafe partial void RegisterCallback(
    delegate* unmanaged[Cdecl]<int, nint, int> callback,
    nint context);

// Callback implementation
[UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
static int MyCallback(int value, nint context)
{
    // Process value
    return 0;
}

// Registration
unsafe
{
    RegisterCallback(&MyCallback, nint.Zero);
}

```text

**Alternative: Delegate-based callbacks (when managed state is needed)**

```csharp

// Define delegate matching native signature
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
delegate int NativeCallback(int value, nint context);

[LibraryImport("mylib")]
internal static partial void RegisterCallbackDelegate(
    NativeCallback callback,
    nint context);

// Usage -- prevent GC collection during native use
static NativeCallback? s_callback;

static void Setup()
{
    s_callback = new NativeCallback(MyManagedCallback);
    RegisterCallbackDelegate(s_callback, nint.Zero);
    // Keep s_callback alive as long as native code may call it
}

static int MyManagedCallback(int value, nint context)
{
    return value * 2;
}

```text

### SafeHandle for Resource Lifetime

Use `SafeHandle` subclasses to manage native resource lifetimes instead of raw `IntPtr`/`nint`. This prevents resource leaks and use-after-free bugs.

```csharp

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

// Custom SafeHandle for a native resource
public class NativeResourceHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private NativeResourceHandle() : base(ownsHandle: true) { }

    protected override bool ReleaseHandle()
    {
        NativeApi.CloseResource(handle);
        return true;
    }
}

public static partial class NativeApi
{
    [LibraryImport("mylib")]
    internal static partial NativeResourceHandle OpenResource(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [LibraryImport("mylib")]
    internal static partial void CloseResource(nint handle);

    [LibraryImport("mylib")]
    internal static partial int ReadResource(NativeResourceHandle handle,
        Span<byte> buffer, int count);
}

```text

---

## Cross-Platform Data Type Mapping

Map C/C++ types to .NET types carefully. Some C types have platform-dependent sizes.

### Fixed-Size Types

| C/C++ Type | .NET Type | Size |
|------------|-----------|------|
| `int8_t` / `char` | `sbyte` | 1 byte |
| `uint8_t` / `unsigned char` | `byte` | 1 byte |
| `int16_t` / `short` | `short` | 2 bytes |
| `uint16_t` / `unsigned short` | `ushort` | 2 bytes |
| `int32_t` / `int` | `int` | 4 bytes |
| `uint32_t` / `unsigned int` | `uint` | 4 bytes |
| `int64_t` / `long long` | `long` | 8 bytes |
| `uint64_t` / `unsigned long long` | `ulong` | 8 bytes |
| `float` | `float` | 4 bytes |
| `double` | `double` | 8 bytes |

### Platform-Dependent Types

| C/C++ Type | .NET Type | Notes |
|------------|-----------|-------|
| `size_t` / `ptrdiff_t` | `nint` / `nuint` | Pointer-sized |
| `void*` / pointer types | `nint` or `void*` | Pointer-sized |
| `long` (C/C++) | `CLong` (.NET 6+) | 4 bytes on Windows, 8 bytes on Unix 64-bit |
| `unsigned long` | `CULong` (.NET 6+) | Same platform variance as `long` |
| Windows `BOOL` | `int` | 4 bytes (not `bool`) |
| Windows `BOOLEAN` | `byte` | 1 byte |

Do not use C# `long` for C/C++ `long` -- they have different sizes on Unix 64-bit. Use `CLong`/`CULong` for portable interop.

---

## Agent Gotchas

1. **Do not use `[DllImport]` in new .NET 7+ code without justification.** Use `[LibraryImport]` which generates marshalling at compile time. Only fall back to `[DllImport]` when SYSLIB1054 analyzer indicates incompatibility.
2. **Do not assume `bool` marshals as 1 byte.** .NET marshals `bool` as a 4-byte Windows `BOOL` by default. Use `[MarshalAs(UnmanagedType.U1)]` for C `_Bool`/`bool`, or `[MarshalAs(UnmanagedType.Bool)]` for Windows `BOOL` explicitly.
3. **Do not use C# `long` to interop with C/C++ `long`.** C `long` is 4 bytes on Windows but 8 bytes on 64-bit Unix. Use `CLong`/`CULong` (.NET 6+) for cross-platform correctness.
4. **Do not use `StringBuilder` for output string buffers.** `[LibraryImport]` does not support `StringBuilder` at all, and with `[DllImport]` it allocates multiple intermediate copies. Use `char[]` or `byte[]` from `ArrayPool` instead.
5. **Do not use `[LibraryImport]` or `[DllImport]` for WASM.** WebAssembly does not support traditional P/Invoke. For JavaScript interop in WASM, see [skill:dotnet-aot-wasm].
6. **Do not use dynamic library loading on iOS.** iOS prohibits loading dynamic libraries at runtime. Use `"__Internal"` as the library name for statically linked native code.
7. **Do not use `System.Delegate` fields in interop structs.** Use typed delegates or unmanaged function pointers (`delegate* unmanaged`). Untyped delegates can destabilize the runtime during marshalling.
8. **Do not forget to keep delegate instances alive during native use.** The GC may collect a delegate that native code still references. Store delegates in a static field or use `GCHandle` for the duration of native callbacks.

---

## Prerequisites

- .NET 7+ SDK for `[LibraryImport]` source generation
- .NET Core 3.0+ for `NativeLibrary` API
- Native libraries compiled for each target platform/architecture
- For iOS: Xcode with native static libraries linked via `NativeReference`
- For Android: native `.so` files for each target ABI (arm64-v8a, x86_64)

---

## References

- [Platform Invoke (P/Invoke)](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)
- [Native interoperability best practices](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/best-practices)
- [LibraryImport source generation](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke-source-generation)
- [Type marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/type-marshalling)
- [Customizing struct marshalling](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/customize-struct-marshalling)
- [NativeLibrary class](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.nativelibrary)
````

## Code Navigation (Serena MCP)

**Primary approach:** Use Serena symbol operations for efficient code navigation:

1. **Find definitions**: `serena_find_symbol` instead of text search
2. **Understand structure**: `serena_get_symbols_overview` for file organization
3. **Track references**: `serena_find_referencing_symbols` for impact analysis
4. **Precise edits**: `serena_replace_symbol_body` for clean modifications

**When to use Serena vs traditional tools:**

- **Use Serena**: Navigation, refactoring, dependency analysis, precise edits
- **Use Read/Grep**: Reading full files, pattern matching, simple text operations
- **Fallback**: If Serena unavailable, traditional tools work fine

**Example workflow:**

```text
# Instead of:
Read: src/Services/OrderService.cs
Grep: "public void ProcessOrder"

# Use:
serena_find_symbol: "OrderService/ProcessOrder"
serena_get_symbols_overview: "src/Services/OrderService.cs"
```
