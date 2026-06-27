- The same `PackageName` and `Publisher` are used
- The `Version` is incremented
- Both old and new packages are hosted on the same server

No additional configuration is needed for differential updates.

---

## MSIX Bundle Format

MSIX bundles (`.msixbundle`) package multiple architecture-specific MSIX packages into a single downloadable artifact. Windows automatically installs the correct architecture.

### Creating a Bundle

```powershell

# Build for multiple architectures
dotnet publish -c Release -r win-x64 /p:GenerateAppxPackageOnBuild=true
dotnet publish -c Release -r win-arm64 /p:GenerateAppxPackageOnBuild=true

# Find MakeAppx.exe dynamically (SDK version varies by machine)
$makeappx = Get-ChildItem "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\MakeAppx.exe" |
  Sort-Object { [version]($_.Directory.Parent.Name) } -Descending |
  Select-Object -First 1 -ExpandProperty FullName

# Create bundle
& $makeappx bundle /d "AppPackages" /p "MyApp_1.0.0.0.msixbundle"

```text

### MSBuild Bundle Generation

```xml

<PropertyGroup>
  <!-- Auto-generate bundle during build -->
  <AppxBundle>Always</AppxBundle>
  <AppxBundlePlatforms>x64|arm64</AppxBundlePlatforms>
</PropertyGroup>

```text

### Bundle Layout

```text

MyApp_1.0.0.0.msixbundle
  MyApp_1.0.0.0_x64.msix
  MyApp_1.0.0.0_arm64.msix

```text

---

## CI/CD MSIX Build Steps

### GitHub Actions MSIX Build

```yaml

# MSIX-specific build steps (embed in your CI workflow)
# For pipeline structure, see [skill:dotnet-gha-patterns]
jobs:
  build-msix:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Decode signing certificate
        run: |
          $pfxBytes = [System.Convert]::FromBase64String("${{ secrets.SIGNING_CERT_BASE64 }}")
          [System.IO.File]::WriteAllBytes("signing-cert.pfx", $pfxBytes)
        shell: pwsh

      - name: Build MSIX package
        run: |
          dotnet publish --configuration Release --runtime win-x64 `
            /p:GenerateAppxPackageOnBuild=true `
            /p:AppxPackageSigningEnabled=true `
            /p:PackageCertificateKeyFile="${{ github.workspace }}\signing-cert.pfx" `
            /p:PackageCertificatePassword="${{ secrets.CERT_PASSWORD }}"
        shell: pwsh

      - name: Upload MSIX artifact
        uses: actions/upload-artifact@v4
        with:
          name: msix-package
          path: AppPackages/**/*.msix

      - name: Clean up certificate
        if: always()
        run: Remove-Item -Path "signing-cert.pfx" -ErrorAction SilentlyContinue
        shell: pwsh

```bash

### Azure DevOps MSIX Build

```yaml

# MSIX-specific build steps (embed in your ADO pipeline)
# For pipeline structure, see [skill:dotnet-ado-patterns]
steps:
  - task: UseDotNet@2
    inputs:
      packageType: 'sdk'
      version: '8.0.x'

  - task: DownloadSecureFile@1
    name: signingCert
    inputs:
      secureFile: 'signing-cert.pfx'

  - task: DotNetCoreCLI@2
    displayName: 'Build MSIX'
    inputs:
      command: 'publish'
      publishWebProjects: false
      projects: '**/MyApp.csproj'
      arguments: >-
        --configuration Release
        --runtime win-x64
        /p:GenerateAppxPackageOnBuild=true
        /p:AppxPackageSigningEnabled=true
        /p:PackageCertificateKeyFile=$(signingCert.secureFilePath)
        /p:PackageCertificatePassword=$(CERT_PASSWORD)

  - task: PublishBuildArtifacts@1
    inputs:
      pathToPublish: 'AppPackages'
      artifactName: 'msix-package'

```text

### AOT + MSIX

MSIX packages can contain AOT-compiled binaries for faster startup and smaller runtime footprint. Combine `PublishAot` with MSIX packaging:

```xml

<PropertyGroup>
  <PublishAot>true</PublishAot>
  <WindowsPackageType>MSIX</WindowsPackageType>
</PropertyGroup>

```text

For AOT MSBuild configuration details (ILLink descriptors, trimming options, platform considerations), see [skill:dotnet-native-aot].

---

## Windows SDK Version Requirements

| Feature | Minimum Windows Version | Build Number |
|---------|------------------------|--------------|
| MSIX with Windows App SDK | Windows 10 | Build 19041 (2004) |
| App Installer protocol | Windows 10 | Build 1709 (Fall Creators Update) |
| Auto-update (OnLaunch) | Windows 10 | Build 1709 |
| Background auto-update | Windows 10 | Build 1803 (April 2018 Update) |
| ForceUpdateFromAnyVersion | Windows 10 | Build 1809 |
| MSIX bundle format | Windows 10 | Build 1709 |
| Optional packages | Windows 10 | Build 1709 |
| Modification packages | Windows 10 | Build 1809 |
| App Installer file hosting | Windows 10 | Build 1709 |
| Microsoft Store submission | Windows App SDK 1.6+ | N/A |

### Target Platform Version Configuration

```xml

<PropertyGroup>
  <!-- Minimum supported version (features below this are unavailable) -->
  <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>

  <!-- Maximum tested version (for adaptive version checks) -->
  <TargetPlatformVersion>10.0.22621.0</TargetPlatformVersion>
</PropertyGroup>

```text

---

## Agent Gotchas

1. **The manifest `Publisher` must exactly match the signing certificate Subject** -- mismatches cause `SignTool Error: SignerSign() failed` at build or sign time.

1. **Self-signed certificates require manual trust installation** -- users must install the certificate to `Trusted People` or `Trusted Root Certification Authorities` before the MSIX will install.

1. **Never commit PFX files or certificate passwords to source control** -- store certificates as CI secrets (GitHub Secrets, Azure DevOps Secure Files) and decode them during the build pipeline.

1. **`AppxBundle=Auto` produces a bundle only when multiple architectures are built** -- for single-architecture builds, it produces a flat `.msix` file, not a bundle.

1. **MSIX apps run in a container-like sandbox** -- file system access is virtualized. Apps writing to `AppData` get redirected to the package-specific location. Use `ApplicationData.Current` APIs, not hardcoded paths.

1. **Store submission uses `.msixupload` not `.msix`** -- set `/p:UapAppxPackageBuildMode=StoreUpload` to generate the correct upload format.

1. **CI builds on `windows-latest` include the Windows SDK** -- no separate SDK installation step is needed for `signtool.exe` and `MakeAppx.exe`.

1. **Do not hardcode TFM paths in CI examples** -- use variable references (e.g., `${{ github.workspace }}`) so examples work across .NET versions.
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
