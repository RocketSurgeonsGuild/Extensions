
```powershell

**mytool.nuspec:**

```xml

<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.xmldata.org/2004/07/nuspec">
  <metadata>
    <id>mytool</id>
    <version>1.2.3</version>
    <title>MyTool</title>
    <authors>My Org</authors>
    <projectUrl>https://github.com/myorg/mytool</projectUrl>
    <license type="expression">MIT</license>
    <description>A CLI tool for managing widgets.</description>
    <tags>cli dotnet tools</tags>
  </metadata>
</package>

```text

**tools/chocolateyInstall.ps1:**

```powershell

$ErrorActionPreference = 'Stop'

$packageArgs = @{
  packageName    = 'mytool'
  url64bit       = 'https://github.com/myorg/mytool/releases/download/v1.2.3/mytool-1.2.3-win-x64.zip'
  checksum64     = 'ABC123...'
  checksumType64 = 'sha256'
  unzipLocation  = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)"
}

Install-ChocolateyZipPackage @packageArgs

```bash

### Building and Publishing

```powershell

# Pack the .nupkg
choco pack mytool.nuspec

# Test locally
choco install mytool --source="." --force

# Push to Chocolatey Community Repository
choco push mytool.1.2.3.nupkg --source https://push.chocolatey.org/ --api-key $env:CHOCO_API_KEY

```text

---

## dotnet tool (Global and Local)

`dotnet tool` is the simplest distribution for .NET developers. Tools are distributed as NuGet packages.

### Project Configuration for Tool Packaging

```xml

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>

    <!-- Tool packaging properties -->
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>mytool</ToolCommandName>
    <PackageId>MyOrg.MyTool</PackageId>
    <Version>1.2.3</Version>
    <Description>A CLI tool for managing widgets</Description>
    <Authors>My Org</Authors>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/myorg/mytool</PackageProjectUrl>
    <PackageReadmeFile>README.md</PackageReadmeFile>
  </PropertyGroup>

  <ItemGroup>
    <None Include="../../README.md" Pack="true" PackagePath="/" />
  </ItemGroup>
</Project>

```markdown

### Building and Publishing

```bash

# Pack the tool
dotnet pack -c Release

# Publish to NuGet.org
dotnet nuget push bin/Release/MyOrg.MyTool.1.2.3.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key "$NUGET_API_KEY"

```json

### Installing dotnet Tools

```bash

# Global tool (available system-wide)
dotnet tool install -g MyOrg.MyTool

# Local tool (per-project, tracked in .config/dotnet-tools.json)
dotnet new tool-manifest  # first time only
dotnet tool install MyOrg.MyTool

# Update
dotnet tool update -g MyOrg.MyTool

# Run local tool
dotnet tool run mytool
# or just:
dotnet mytool

```text

### Global vs Local Tools

| Aspect             | Global Tool                 | Local Tool                         |
| ------------------ | --------------------------- | ---------------------------------- |
| Scope              | System-wide (per user)      | Per-project directory              |
| Install location   | `~/.dotnet/tools`           | `.config/dotnet-tools.json`        |
| Version management | Manual update               | Tracked in source control          |
| CI/CD              | Must install before use     | `dotnet tool restore` restores all |
| Best for           | Personal productivity tools | Project-specific build tools       |

---

## NuGet Distribution

For tools distributed as NuGet packages (either as `dotnet tool` or standalone):

### Package Metadata

```xml

<PropertyGroup>
  <PackageId>MyOrg.MyTool</PackageId>
  <Version>1.2.3</Version>
  <Description>A CLI tool for managing widgets</Description>
  <Authors>My Org</Authors>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageProjectUrl>https://github.com/myorg/mytool</PackageProjectUrl>
  <PackageReadmeFile>README.md</PackageReadmeFile>
  <PackageTags>cli;tools;widgets</PackageTags>
  <RepositoryUrl>https://github.com/myorg/mytool</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
</PropertyGroup>

```text

### Publishing to NuGet.org

```bash

# Pack
dotnet pack -c Release -o ./nupkgs

# Push (use env var for API key -- never hardcode)
dotnet nuget push ./nupkgs/MyOrg.MyTool.1.2.3.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key "$NUGET_API_KEY"

```json

### Private Feed Distribution

```bash

# Push to a private feed (Azure Artifacts, GitHub Packages, etc.)
dotnet nuget push ./nupkgs/MyOrg.MyTool.1.2.3.nupkg \
  --source https://pkgs.dev.azure.com/myorg/_packaging/myfeed/nuget/v3/index.json \
  --api-key "$AZURE_ARTIFACTS_PAT"

```json

---

## Package Format Comparison

| Format           | Platform       | Requires .NET   | Auto-Update          | Difficulty |
| ---------------- | -------------- | --------------- | -------------------- | ---------- |
| Homebrew formula | macOS, Linux   | No (binary tap) | `brew upgrade`       | Medium     |
| apt/deb          | Debian/Ubuntu  | No (AOT binary) | Via apt repo         | Medium     |
| winget           | Windows 10+    | No (portable)   | `winget upgrade`     | Medium     |
| Scoop            | Windows        | No (portable)   | `scoop update`       | Low        |
| Chocolatey       | Windows        | No              | `choco upgrade`      | Medium     |
| dotnet tool      | Cross-platform | Yes (SDK)       | `dotnet tool update` | Low        |
| NuGet (library)  | Cross-platform | Yes (SDK)       | NuGet restore        | Low        |

---

## Agent Gotchas

1. **Do not hardcode SHA-256 hashes in package manifests.** Generate checksums from actual release artifacts, not
   placeholder values. All package managers validate checksums against downloaded files.
2. **Do not use `InstallerType: exe` for portable CLI tools in winget.** Use `InstallerType: zip` with
   `NestedInstallerType: portable` for standalone executables. The `exe` type implies an installer with silent flags.
3. **Do not forget `PackAsTool` for dotnet tool projects.** Without `<PackAsTool>true</PackAsTool>`, `dotnet pack`
   produces a library package, not an installable tool.
4. **Do not hardcode API keys in packaging scripts.** Use environment variable references (`$NUGET_API_KEY`,
   `$env:CHOCO_API_KEY`) with a comment noting CI secret configuration.
5. **Do not mix Homebrew formula and cask for the same CLI tool.** Pure CLI tools should use formulae. Casks are for GUI
   applications with macOS app bundles.
6. **Do not skip the `test` block in Homebrew formulae.** Homebrew CI runs formula tests. A missing test block causes
   review rejection. At minimum, test `--version` output.

---



## Code Navigation (Serena MCP)

**Primary approach:** Use Serena symbol operations for efficient code navigation:

1. **Find definitions**: `serena_find_symbol` instead of text search
2. **Understand structure**: `serena_get_symbols_overview` for file organization
3. **Track references**: `serena_find_referencing_symbols` for impact analysis
4. **Precise edits**: `serena_replace_symbol_body` for clean modifications

**When to use Serena vs traditional tools:**
- ✅ **Use Serena**: Navigation, refactoring, dependency analysis, precise edits
- ✅ **Use Read/Grep**: Reading full files, pattern matching, simple text operations
- ✅ **Fallback**: If Serena unavailable, traditional tools work fine

**Example workflow:**
```text
# Instead of:
Read: src/Services/OrderService.cs
Grep: "public void ProcessOrder"

# Use:
serena_find_symbol: "OrderService/ProcessOrder"
serena_get_symbols_overview: "src/Services/OrderService.cs"
```
## References

- [Homebrew Formula Cookbook](https://docs.brew.sh/Formula-Cookbook)
- [Homebrew Taps](https://docs.brew.sh/Taps)
- [dpkg-deb manual](https://man7.org/linux/man-pages/man1/dpkg-deb.1.html)
- [winget manifest schema](https://learn.microsoft.com/en-us/windows/package-manager/package/manifest)
- [winget-pkgs repository](https://github.com/microsoft/winget-pkgs)
- [Scoop Wiki](https://github.com/ScoopInstaller/Scoop/wiki)
- [Chocolatey package creation](https://docs.chocolatey.org/en-us/create/create-packages)
- [.NET tool packaging](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create)
- [NuGet publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
````
