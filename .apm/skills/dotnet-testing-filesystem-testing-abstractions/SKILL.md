---
name: dotnet-testing-filesystem-testing-abstractions
category: testing
subcategory: specialized
description: |
  Specialized skill for testing file system operations using System.IO.Abstractions. Use when you need to test File, Directory, Path operations, or simulate file system. Covers IFileSystem, MockFileSystem, file read/write testing, directory operation testing, etc.
  Keywords: file testing, filesystem, file testing, file system testing, IFileSystem, MockFileSystem, System.IO.Abstractions, File.ReadAllText, File.WriteAllText, Directory.CreateDirectory, Path.Combine, mock file system, file abstraction
targets: ['*']
license: MIT
metadata:
  author: Kevin Tseng
  version: '1.0.0'
  tags: '.NET, testing, IFileSystem, MockFileSystem, file testing'
  related_skills: 'nsubstitute-mocking, unit-test-fundamentals, datetime-testing-timeprovider'
claudecode: {}
opencode: {}
codexcli:
  short-description: '.NET skill guidance for dotnet-testing-filesystem-testing-abstractions'
copilot: {}
geminicli: {}
antigravity: {}
---

Source: kevintsengtw/dotnet-testing-agent-skills (MIT). Ported into dotnet-agent-harness.

# File System Testing: Using System.IO.Abstractions to Simulate File Operations

## Applicable Scenarios

Use this skill when asked to perform the following tasks:

- Refactor code directly using `System.IO.File`, `System.IO.Directory` and other static classes
- Write unit tests for code involving file read/write, directory operations
- Use MockFileSystem to simulate various file system states
- Test exception scenarios like insufficient file permissions, file not found
- Design testable file processing service architecture

## Core Principles

### 1. Fundamental Problem of File System Dependencies

Traditional code directly using `System.IO` static classes is difficult to test, reasons include:

- **Speed Issues**: Actual disk IO is 10-100x slower than memory operations
- **Environment Dependency**: Test results affected by file system state, permissions, paths
- **Side Effects**: Tests leave traces on disk, affecting other tests
- **Concurrency Issues**: Multiple tests operating on same file create race conditions
- **Error Simulation Difficulty**: Difficult to simulate insufficient permissions, insufficient disk space, etc.

### 2. System.IO.Abstractions Solution

This is a package that wraps System.IO static classes into interfaces, supporting dependency injection and test doubles.

**Core Interface Architecture**:

````csharp
public interface IFileSystem
{
    IFile File { get; }
    IDirectory Directory { get; }
    IFileInfo FileInfo { get; }
    IDirectoryInfo DirectoryInfo { get; }
    IPath Path { get; }
    IDriveInfo DriveInfo { get; }
}
```text

**Required NuGet Packages**:

```xml
<!-- Production environment -->
<PackageReference Include="System.IO.Abstractions" Version="21.*" />

<!-- Test project -->
<PackageReference Include="System.IO.Abstractions.TestingHelpers" Version="21.*" />
```text

### 3. Refactoring Steps

**Step 1**: Change code directly using static classes to depend on `IFileSystem`

```csharp
// ❌ Before refactoring (not testable)
public class ConfigService
{
    public string LoadConfig(string path)
    {
        return File.ReadAllText(path);
    }
}

// ✅ After refactoring (testable)
public class ConfigService
{
    private readonly IFileSystem _fileSystem;

    public ConfigService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public string LoadConfig(string path)
    {
        return _fileSystem.File.ReadAllText(path);
    }
}
```text

**Step 2**: Register real implementation in DI container

```csharp
// Program.cs
services.AddSingleton<IFileSystem, FileSystem>();
services.AddScoped<ConfigService>();
```text

**Step 3**: Use MockFileSystem in tests

```csharp
var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
{
    ["config.json"] = new MockFileData("{ \"key\": \"value\" }")
});
var service = new ConfigService(mockFs);
```text

## MockFileSystem Testing Patterns

### Pattern 1: Default File State

```csharp
[Fact]
public async Task LoadConfig_File_Exists_Should_Return_Content()
{
    // Arrange - Create default file system state
    var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
    {
        ["config.json"] = new MockFileData("{ \"key\": \"value\" }"),
        [@"C:\data\users.csv"] = new MockFileData("Name,Age\nJohn,25"),
        [@"C:\logs\"] = new MockDirectoryData()  // Empty directory
    });

    var service = new ConfigService(mockFileSystem);

    // Act
    var result = await service.LoadConfigAsync("config.json");

    // Assert
    result.Should().Contain("key");
}
```text

### Pattern 2: Verify Write Results

```csharp
[Fact]
public async Task SaveConfig_Specified_Content_Should_Write_Correctly()
{
    // Arrange
    var mockFileSystem = new MockFileSystem();
    var service = new ConfigService(mockFileSystem);

    // Act
    await service.SaveConfigAsync("output.json", "{ \"saved\": true }");

    // Assert - Verify final state of file system
    mockFileSystem.File.Exists("output.json").Should().BeTrue();
    var content = await mockFileSystem.File.ReadAllTextAsync("output.json");
    content.Should().Contain("saved");
}
```text

### Pattern 3: Test Directory Operations

```csharp
[Fact]
public void CopyFile_Target_Directory_Not_Exists_Should_Auto_Create()
{
    // Arrange
    var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
    {
        [@"C:\source\file.txt"] = new MockFileData("content")
    });
    var service = new FileManagerService(mockFileSystem);

    // Act
    service.CopyFileToDirectory(@"C:\source\file.txt", @"C:\target\subfolder");

    // Assert
    mockFileSystem.Directory.Exists(@"C:\target\subfolder").Should().BeTrue();
    mockFileSystem.File.Exists(@"C:\target\subfolder\file.txt").Should().BeTrue();
}
```text

### Pattern 4: Use NSubstitute to Simulate Errors

When needing to simulate specific exceptions, MockFileSystem has limited support, can use NSubstitute:

```csharp
[Fact]
public void TryReadFile_Insufficient_Permissions_Should_Return_False()
{
    // Arrange
    var mockFileSystem = Substitute.For<IFileSystem>();
    var mockFile = Substitute.For<IFile>();

    mockFileSystem.File.Returns(mockFile);
    mockFile.Exists("protected.txt").Returns(true);
    mockFile.ReadAllText("protected.txt")
            .Throws(new UnauthorizedAccessException("Access denied"));

    var service = new FilePermissionService(mockFileSystem);

    // Act
    var result = service.TryReadFile("protected.txt", out var content);

    // Assert
    result.Should().BeFalse();
    content.Should().BeNull();
}
```text

## Advanced Testing Techniques

### Stream Operation Testing

```csharp
[Fact]
public async Task CountLines_Multi_Line_File_Should_Return_Correct_Count()
{
    // Arrange
    var content = "Line 1\nLine 2\nLine 3\nLine 4";
    var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
    {
        ["data.txt"] = new MockFileData(content)
    });

    var processor = new StreamProcessorService(mockFileSystem);

    // Act
    var result = await processor.CountLinesAsync("data.txt");

    // Assert
    result.Should().Be(4);
}
```text

### File Information Testing

```csharp
[Fact]
public void GetFileInfo_File_Exists_Should_Return_Correct_Info()
{
    // Arrange
    var content = "Hello, World!";
    var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
    {
        [@"C:\test.txt"] = new MockFileData(content)
    });

    var service = new FileManagerService(mockFileSystem);

    // Act
    var info = service.GetFileInfo(@"C:\test.txt");

    // Assert
    info.Should().NotBeNull();
    info!.Name.Should().Be("test.txt");
    info.Size.Should().Be(content.Length);
}
```text

### Backup File Testing

```csharp
[Fact]
public void BackupFile_File_Exists_Should_Create_Timestamp_Backup()
{
    // Arrange
    var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
    {
        [@"C:\data\important.txt"] = new MockFileData("important data")
    });

    var service = new FileManagerService(mockFileSystem);

    // Act
    var backupPath = service.BackupFile(@"C:\data\important.txt");

    // Assert
    backupPath.Should().StartWith(@"C:\data\important_");
    backupPath.Should().EndWith(".txt");
    mockFileSystem.File.Exists(backupPath).Should().BeTrue();
}
```text

## Best Practices

### ✅ Should Do

1. **Use Path.Combine to handle paths**:

   ```csharp
   var path = _fileSystem.Path.Combine("configs", "app.json");
```text

2. **Defensively check file existence**:

   ```csharp
   if (!_fileSystem.File.Exists(filePath))
   {
       return defaultValue;
   }
```text

3. **Auto-create necessary directories**:

   ```csharp
   var dir = _fileSystem.Path.GetDirectoryName(filePath);
   if (!string.IsNullOrEmpty(dir) && !_fileSystem.Directory.Exists(dir))
   {
       _fileSystem.Directory.CreateDirectory(dir);
   }
```text

4. **Properly handle various IO exceptions**:

   ```csharp
   try
   {
       return await _fileSystem.File.ReadAllTextAsync(path);
   }
   catch (UnauthorizedAccessException) { /* insufficient permissions */ }
   catch (IOException) { /* file locked */ }
   catch (DirectoryNotFoundException) { /* directory not found */ }
```text

5. **Use independent MockFileSystem for each test**:

   ```csharp
   public class ServiceTests
   {
       [Fact]
       public void Test1()
       {
           var mockFs = new MockFileSystem(); // Independent instance
       }

       [Fact]
       public void Test2()
       {
           var mockFs = new MockFileSystem(); // Independent instance
       }
   }
```text

### ❌ Should Avoid

1. **Hardcode path separators**:

   ```csharp
   // ❌ Don't do this
   var path = "configs\\app.json";  // Windows only
   var path = "configs/app.json";   // Unix only

   // ✅ Should do this
   var path = _fileSystem.Path.Combine("configs", "app.json");
```text

2. **Use real file system in unit tests**:

   ```csharp
   // ❌ This is not a unit test
   var realFs = new FileSystem();

   // ✅ Unit tests should use MockFileSystem
   var mockFs = new MockFileSystem();
```text

3. **Ignore exception handling**:

   ```csharp
   // ❌ Don't assume file always exists
   var content = _fileSystem.File.ReadAllText(path);

   // ✅ Add existence check and exception handling
   if (_fileSystem.File.Exists(path))
   {
       try { return _fileSystem.File.ReadAllText(path); }
       catch (IOException) { return defaultValue; }
   }
```text

## Performance Considerations

### MockFileSystem Advantages

- **Speed**: 10-100x faster than real file operations
- **Reliability**: Not affected by disk state
- **Isolation**: Complete isolation between tests
- **Error Simulation**: Can precisely simulate various exception scenarios

### Memory Usage Recommendations

- Only create files necessary for testing
- Avoid simulating oversized files in tests
- For large file processing logic, use moderately sized test data:

```csharp
// ✅ Moderately sized test data
var testContent = string.Join("\n",
    Enumerable.Range(1, 1000).Select(i => $"Line {i}"));
mockFileSystem.AddFile("test.txt", new MockFileData(testContent));
```text

## Practical Integration Examples

### Configuration File Management Service

See `templates/configmanager-service.cs` for complete implementation, including:

- Configuration file load and save
- JSON serialization and deserialization
- Auto-create directories
- Configuration file backup functionality

### File Management Service

See `templates/filemanager-service.cs` for implementation, including:

- File copy and backup
- Directory operations
- File information query
- Error handling patterns

## Reference Resources

### Original Articles

This skill content is distilled from the "Old School Software Engineer's Testing Practice - 30 Day Challenge" article series:

- **Day 17 - File and IO Testing: Using System.IO.Abstractions to Simulate File System**
  - Article: https://ithelp.ithome.com.tw/articles/10375981
  - Sample Code: https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day17

### Official Documentation

- [System.IO.Abstractions GitHub](https://github.com/TestableIO/System.IO.Abstractions)
- [System.IO.Abstractions NuGet](https://www.nuget.org/packages/System.IO.Abstractions/)
- [TestingHelpers NuGet](https://www.nuget.org/packages/System.IO.Abstractions.TestingHelpers/)

### Related Skills

- `nsubstitute-mocking` - Test doubles and mocking
- `unit-test-fundamentals` - Unit testing basics
````
