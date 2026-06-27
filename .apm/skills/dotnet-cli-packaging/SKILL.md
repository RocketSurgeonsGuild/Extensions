---
name: dotnet-cli-packaging
category: operations
subcategory: release
description: Publishes to package managers. Homebrew, apt/deb, winget, Scoop, Chocolatey manifests.
license: MIT
targets: ['*']
tags: [foundation, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for foundation tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-cli-packaging

Multi-platform packaging for .NET CLI tools: Homebrew formula authoring (binary tap and cask), apt/deb packaging with
`dpkg-deb`, winget manifest YAML schema and PR submission to `winget-pkgs`, Scoop manifest JSON, Chocolatey package
creation, `dotnet tool` global/local packaging, and NuGet distribution.

**Version assumptions:** .NET 8.0+ baseline. Package manager formats are stable across .NET versions.

## Scope

- Homebrew formula authoring (binary tap and cask)
- apt/deb packaging with dpkg-deb
- winget manifest YAML schema and PR submission
- Scoop manifest JSON for Windows
- Chocolatey package creation
- dotnet tool global/local packaging and NuGet distribution

## Out of scope

- CLI distribution strategy (AOT vs framework-dependent decision) -- see [skill:dotnet-cli-distribution]
- Release CI/CD pipeline that automates packaging -- see [skill:dotnet-cli-release-pipeline]
- Native AOT compilation -- see [skill:dotnet-native-aot]
- Container-based distribution -- see [skill:dotnet-containers]
- General CI/CD patterns -- see [skill:dotnet-gha-patterns] and [skill:dotnet-ado-patterns]

Cross-references: [skill:dotnet-cli-distribution] for distribution strategy and RID matrix,
[skill:dotnet-cli-release-pipeline] for automated package publishing, [skill:dotnet-native-aot] for AOT binary
production, [skill:dotnet-containers] for container-based distribution, [skill:dotnet-tool-management] for consumer-side
tool installation and manifest management.

---

## Homebrew (macOS / Linux)

Homebrew is the primary package manager for macOS and widely used on Linux. Two distribution formats exist for CLI
tools.

### Binary Tap (Formula)

A formula downloads pre-built binaries per platform. This is the recommended approach for Native AOT CLI tools.

````ruby

# Formula/mytool.rb
class Mytool < Formula
  desc "A CLI tool for managing widgets"
  homepage "https://github.com/myorg/mytool"
  version "1.2.3"
  license "MIT"

  on_macos do
    on_arm do
      url "https://github.com/myorg/mytool/releases/download/v1.2.3/mytool-1.2.3-osx-arm64.tar.gz"
      sha256 "abc123..."
    end
    # Optional: remove on_intel block if not targeting Intel Macs
    on_intel do
      url "https://github.com/myorg/mytool/releases/download/v1.2.3/mytool-1.2.3-osx-x64.tar.gz"
      sha256 "def456..."
    end
  end

  on_linux do
    on_arm do
      url "https://github.com/myorg/mytool/releases/download/v1.2.3/mytool-1.2.3-linux-arm64.tar.gz"
      sha256 "ghi789..."
    end
    on_intel do
      url "https://github.com/myorg/mytool/releases/download/v1.2.3/mytool-1.2.3-linux-x64.tar.gz"
      sha256 "jkl012..."
    end
  end

  def install
    bin.install "mytool"
  end

  test do
    assert_match version.to_s, shell_output("#{bin}/mytool --version")
  end
end

```bash

### Hosting a Tap

A tap is a Git repository containing formulae. Create a repo named `homebrew-tap`:

```text

myorg/homebrew-tap/
  Formula/
    mytool.rb

```text

Users install with:

```bash

brew tap myorg/tap
brew install mytool

```bash

### Homebrew Cask

Casks are for GUI applications or tools with an installer. For pure CLI tools, prefer formulae over casks.

```ruby

# Casks/mytool.rb -- only if the tool has a GUI component
cask "mytool" do
  version "1.2.3"
  sha256 "abc123..."

  url "https://github.com/myorg/mytool/releases/download/v#{version}/mytool-#{version}-osx-arm64.tar.gz"
  name "MyTool"
  homepage "https://github.com/myorg/mytool"

  binary "mytool"
end

```text

---

## apt/deb (Debian/Ubuntu)

### Building a .deb Package with dpkg-deb

Create the package directory structure:

```text

mytool_1.2.3_amd64/
  DEBIAN/
    control
  usr/
    bin/
      mytool

```text

**Control file:**

```text

Package: mytool
Version: 1.2.3
Section: utils
Priority: optional
Architecture: amd64
Maintainer: My Org <dev@myorg.com>
Description: A CLI tool for managing widgets
 MyTool provides fast widget management from the command line.
 Built with .NET Native AOT for zero-dependency execution.
Homepage: https://github.com/myorg/mytool

```bash

**Build the package:**

```bash

#!/bin/bash
set -euo pipefail

VERSION="${1:?Usage: build-deb.sh <version>}"
ARCH="amd64"  # or arm64
PKG_DIR="mytool_${VERSION}_${ARCH}"

mkdir -p "$PKG_DIR/DEBIAN"
mkdir -p "$PKG_DIR/usr/bin"

# Copy the published binary
cp "artifacts/linux-x64/mytool" "$PKG_DIR/usr/bin/mytool"
chmod 755 "$PKG_DIR/usr/bin/mytool"

# Write control file
cat > "$PKG_DIR/DEBIAN/control" << EOF
Package: mytool
Version: ${VERSION}
Section: utils
Priority: optional
Architecture: ${ARCH}
Maintainer: My Org <dev@myorg.com>
Description: A CLI tool for managing widgets
Homepage: https://github.com/myorg/mytool
EOF

# Build the .deb
dpkg-deb --build --root-owner-group "$PKG_DIR"
echo "Built: ${PKG_DIR}.deb"

```text

**RID to Debian architecture mapping:**

| .NET RID      | Debian Architecture |
| ------------- | ------------------- |
| `linux-x64`   | `amd64`             |
| `linux-arm64` | `arm64`             |

### Installing the .deb

```bash

sudo dpkg -i mytool_1.2.3_amd64.deb

```bash

---

## winget (Windows Package Manager)

### Manifest YAML Schema

winget manifests consist of multiple YAML files in a versioned directory structure within the `microsoft/winget-pkgs`
repository.

**Directory structure:**

```text

manifests/
  m/
    MyOrg/
      MyTool/
        1.2.3/
          MyOrg.MyTool.yaml              # Version manifest
          MyOrg.MyTool.installer.yaml    # Installer manifest
          MyOrg.MyTool.locale.en-US.yaml # Locale manifest

```yaml

**Version manifest (MyOrg.MyTool.yaml):**

```yaml

PackageIdentifier: MyOrg.MyTool
PackageVersion: 1.2.3
DefaultLocale: en-US
ManifestType: version
ManifestVersion: 1.9.0

```text

**Installer manifest (MyOrg.MyTool.installer.yaml):**

```yaml

PackageIdentifier: MyOrg.MyTool
PackageVersion: 1.2.3
InstallerType: zip
NestedInstallerType: portable
NestedInstallerFiles:
  - RelativeFilePath: mytool.exe
    PortableCommandAlias: mytool
Installers:
  - Architecture: x64
    InstallerUrl: https://github.com/myorg/mytool/releases/download/v1.2.3/mytool-1.2.3-win-x64.zip
    InstallerSha256: ABC123...
  - Architecture: arm64
    InstallerUrl: https://github.com/myorg/mytool/releases/download/v1.2.3/mytool-1.2.3-win-arm64.zip
    InstallerSha256: DEF456...
ManifestType: installer
ManifestVersion: 1.9.0

```text

**Locale manifest (MyOrg.MyTool.locale.en-US.yaml):**

```yaml

PackageIdentifier: MyOrg.MyTool
PackageVersion: 1.2.3
PackageLocale: en-US
PackageName: MyTool
Publisher: My Org
ShortDescription: A CLI tool for managing widgets
License: MIT
PackageUrl: https://github.com/myorg/mytool
ManifestType: defaultLocale
ManifestVersion: 1.9.0

```text

### Submitting to winget-pkgs

1. Fork `microsoft/winget-pkgs` on GitHub
2. Create the manifest files in the correct directory structure
3. Validate locally: `winget validate --manifest <path>`
4. Submit a PR -- automated checks run against the manifest
5. Microsoft team reviews and merges

See [skill:dotnet-cli-release-pipeline] for automating winget PR creation.

---

## Scoop (Windows)

Scoop is popular among Windows power users. Manifests are JSON files in a bucket repository.

### Scoop Manifest

```json

{
  "version": "1.2.3",
  "description": "A CLI tool for managing widgets",
  "homepage": "https://github.com/myorg/mytool",
  "license": "MIT",
  "architecture": {
    "64bit": {
      "url": "https://github.com/myorg/mytool/releases/download/v1.2.3/mytool-1.2.3-win-x64.zip",
      "hash": "abc123..."
    },
    "arm64": {
      "url": "https://github.com/myorg/mytool/releases/download/v1.2.3/mytool-1.2.3-win-arm64.zip",
      "hash": "def456..."
    }
  },
  "bin": "mytool.exe",
  "checkver": {
    "github": "https://github.com/myorg/mytool"
  },
  "autoupdate": {
    "architecture": {
      "64bit": {
        "url": "https://github.com/myorg/mytool/releases/download/v$version/mytool-$version-win-x64.zip"
      },
      "arm64": {
        "url": "https://github.com/myorg/mytool/releases/download/v$version/mytool-$version-win-arm64.zip"
      }
    }
  }
}

```text

### Hosting a Scoop Bucket

Create a GitHub repo named `scoop-mytool` (or `scoop-bucket`):

```text

myorg/scoop-mytool/
  bucket/
    mytool.json

```json

Users install with:

```powershell

scoop bucket add myorg https://github.com/myorg/scoop-mytool
scoop install mytool

```bash

---

## Chocolatey

Chocolatey is Windows' most established package manager for binary distribution.

### Package Structure

```text

mytool/
  mytool.nuspec
  tools/
    chocolateyInstall.ps1
    LICENSE.txt


## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
