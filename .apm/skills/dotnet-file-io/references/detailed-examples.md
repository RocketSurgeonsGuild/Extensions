{
    // Resolve to absolute, resolving ../ and symlinks
    string fullBase = Path.GetFullPath(basePath);
    string fullPath = Path.GetFullPath(
        Path.Join(fullBase, userPath));

    // OrdinalIgnoreCase: safe cross-platform default.
    // On Linux-only deployments, Ordinal is more precise.
    if (!fullPath.StartsWith(fullBase + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase)
        && !fullPath.Equals(fullBase, StringComparison.OrdinalIgnoreCase))
    {
        throw new UnauthorizedAccessException(
            "Path traversal detected");
    }

    return fullPath;
}

```text

### Cross-Platform Path Separators

Use `Path.DirectorySeparatorChar` (platform-specific) and `Path.AltDirectorySeparatorChar` instead of hardcoded `/` or
`\\`. `Path.Join` and `Path.Combine` handle separator normalization automatically.

---

## Secure Temp Files

`Path.GetTempFileName()` creates a zero-byte file with a predictable name pattern and throws when the temp directory
contains 65,535 `.tmp` files. Use `Path.GetRandomFileName()` instead.

```csharp

// INSECURE: predictable name, may throw IOException
// string tempFile = Path.GetTempFileName();

// SECURE: cryptographically random name, explicit creation
string tempPath = Path.Join(
    Path.GetTempPath(),
    Path.GetRandomFileName());

// CreateNew ensures atomic creation -- fails if file exists
await using var fs = new FileStream(
    tempPath,
    FileMode.CreateNew,
    FileAccess.Write,
    FileShare.None,
    bufferSize: 4096,
    FileOptions.Asynchronous | FileOptions.DeleteOnClose);

await fs.WriteAsync(data, cancellationToken);

```text

`FileOptions.DeleteOnClose` ensures the temp file is removed when the stream is closed. On Windows, the OS guarantees
deletion when the last handle closes. On Linux/macOS, deletion happens during `Dispose` and may not occur if the process
is killed abruptly (SIGKILL).

---

## Cross-Platform Considerations

### Case Sensitivity

| Platform | Default filesystem | Case behavior                               |
| -------- | ------------------ | ------------------------------------------- |
| Windows  | NTFS               | Case-preserving, case-insensitive           |
| macOS    | APFS               | Case-preserving, case-insensitive (default) |
| Linux    | ext4/xfs           | Case-sensitive                              |

Use `StringComparison.OrdinalIgnoreCase` for path comparisons that must work cross-platform. Do not assume case
sensitivity behavior -- check at runtime if needed.

### UnixFileMode (.NET 7+)

```csharp

// Set POSIX permissions on file creation (Linux/macOS only)
await using var fs = new FileStream(path, new FileStreamOptions
{
    Mode = FileMode.Create,
    Access = FileAccess.Write,
    UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite
    // 0600 -- owner read/write only
});

```text

On Windows, `UnixCreateMode` is silently ignored. Do not use it as a security control on Windows -- use ACLs or Windows
security APIs instead.

### File Locking

| Platform | Lock type              | Behavior                                           |
| -------- | ---------------------- | -------------------------------------------------- |
| Windows  | Mandatory              | Other processes cannot read/write locked regions   |
| Linux    | Advisory (flock/fcntl) | Locks are cooperative -- processes can ignore them |
| macOS    | Advisory (flock)       | Same as Linux -- cooperative                       |

`FileShare` flags control sharing on Windows. On Linux/macOS, use `FileStream.Lock` for advisory locking but be aware
that non-cooperating processes can bypass it.

---

## Error Handling

### IOException Hierarchy

```text

IOException
  +-- FileNotFoundException
  +-- DirectoryNotFoundException
  +-- PathTooLongException
  +-- DriveNotFoundException
  +-- EndOfStreamException
  +-- FileLoadException

```text

### HResult Codes for Specific Conditions

```csharp

try
{
    await using var fs = new FileStream(path,
        FileMode.Open, FileAccess.Read);
    // ...
}
catch (IOException ex) when (ex.HResult == unchecked((int)0x80070070))
{
    // ERROR_DISK_FULL (Windows) -- disk full
    logger.LogError("Disk full: {Path}", path);
}
catch (IOException ex) when (ex.HResult == unchecked((int)0x80070020))
{
    // ERROR_SHARING_VIOLATION -- file locked by another process
    logger.LogWarning("File locked: {Path}", path);
}
catch (UnauthorizedAccessException ex)
{
    // Permission denied (not an IOException subclass)
    logger.LogError("Access denied: {Path}", path);
}

```text

### Disk-Full Flush Behavior

Write operations may succeed but buffer data in memory. A disk-full condition can surface at `Flush` or `Dispose` time
rather than at the `Write` call. Always check for exceptions on flush.

```csharp

await using var fs = new FileStream(path,
    FileMode.Create, FileAccess.Write,
    FileShare.None, 4096, FileOptions.Asynchronous);

await fs.WriteAsync(data, cancellationToken);

// IOException (disk full) may throw here, not at WriteAsync
await fs.FlushAsync(cancellationToken);

```text

---

## Buffer Sizing

Guidance based on dotnet/runtime benchmarks and internal FileStream implementation:

| File operation                      | Recommended buffer                  | Rationale                                                    |
| ----------------------------------- | ----------------------------------- | ------------------------------------------------------------ |
| Small file sequential read (< 1 MB) | 4 KB (default)                      | Matches OS page size; FileStream default                     |
| Large file sequential read (> 1 MB) | 64--128 KB                          | Amortizes syscall overhead; diminishing returns above 128 KB |
| Network-attached storage (NFS/SMB)  | 64 KB                               | Larger buffers amortize network round-trips                  |
| SSD random access                   | 4 KB                                | Matches SSD page size; larger buffers waste read-ahead       |
| FileStream sequential scan          | 4 KB + `FileOptions.SequentialScan` | OS read-ahead handles prefetching                            |

`FileOptions.SequentialScan` hints the OS to prefetch data ahead of the read position. It is beneficial for sequential
reads and can degrade performance for random access patterns.

---

## Agent Gotchas

1. **Do not use FileStream async methods without `useAsync: true`** -- without the async flag, `ReadAsync`/`WriteAsync`
   dispatch synchronous I/O to the thread pool, blocking a thread and adding overhead. Always pass `useAsync: true` or
   `FileOptions.Asynchronous`.
2. **Do not use `Path.Combine` with untrusted input** -- `Path.Combine` silently discards the base path when the second
   argument is rooted, enabling path traversal. Use `Path.Join` (.NET Core 2.1+) and validate the resolved path is under
   the intended base directory.
3. **Do not use `Path.GetTempFileName()`** -- it creates predictable filenames and throws at 65,535 files. Use
   `Path.GetRandomFileName()` with `FileMode.CreateNew` for secure, atomic temp file creation.
4. **Do not ignore FileSystemWatcher duplicate events** -- editors and tools trigger multiple events for a single
   logical change. Implement debouncing with a timer or Channel<T> throttle.
5. **Do not rely on FileSystemWatcher alone for reliable change detection** -- buffer overflows lose events silently.
   Handle the `Error` event and implement periodic rescan as a fallback.
6. **Do not assume file locking is mandatory on Linux/macOS** -- `FileStream.Lock` and `FileShare` flags use advisory
   locking on Unix, which non-cooperating processes can bypass. Design protocols accordingly.
7. **Do not catch only `IOException` and ignore `UnauthorizedAccessException`** -- permission errors throw
   `UnauthorizedAccessException`, which does not inherit from `IOException`. Handle both in file access error handling.
8. **Do not assume `WriteAsync` reports disk-full errors immediately** -- data may be buffered. Disk-full `IOException`
   can surface at `FlushAsync` or `Dispose`. Always handle exceptions on flush.

---

## References

- [File and stream I/O overview](https://learn.microsoft.com/en-us/dotnet/standard/io/)
- [.NET 6 file I/O improvements](https://devblogs.microsoft.com/dotnet/file-io-improvements-in-dotnet-6/)
- [RandomAccess API](https://learn.microsoft.com/en-us/dotnet/api/system.io.randomaccess)
- [FileSystemWatcher](https://learn.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher)
- [Memory-mapped files](https://learn.microsoft.com/en-us/dotnet/standard/io/memory-mapped-files)
- [Path traversal prevention (OWASP)](https://owasp.org/www-community/attacks/Path_Traversal)
- [Secure temp files](https://docs.datadoghq.com/security/code_security/static_analysis/static_analysis_rules/csharp-security/unsafe-temp-file/)
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
