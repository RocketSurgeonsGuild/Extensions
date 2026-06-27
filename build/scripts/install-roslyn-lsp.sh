#!/usr/bin/env bash
# Installs the Microsoft Roslyn language server (Microsoft.CodeAnalysis.LanguageServer)
# for the current RID into <repo>/.roslyn-lsp and drops a `roslyn-lsp` wrapper on PATH.
#
# The server is not published to nuget.org and ships only as a framework-dependent DLL
# (run via `dotnet ...LanguageServer.dll`), so it is fetched from the public Azure DevOps
# vs-impl feed and wrapped. Version is pinned via ROSLYN_LSP_VERSION (mise [env]).
set -euo pipefail

VERSION="${ROSLYN_LSP_VERSION:?ROSLYN_LSP_VERSION must be set (see .config/mise.toml [env])}"
ROOT="${MISE_PROJECT_ROOT:-$(git rev-parse --show-toplevel)}"
DEST="$ROOT/.roslyn-lsp"
FEED="https://pkgs.dev.azure.com/azure-public/vside/_packaging/vs-impl/nuget/v3/flat2"

os="$(uname -s)"
arch="$(uname -m)"
case "$os" in
    Darwin) case "$arch" in arm64) rid="osx-arm64" ;; x86_64) rid="osx-x64" ;; esac ;;
    Linux) case "$arch" in aarch64 | arm64) rid="linux-arm64" ;; x86_64) rid="linux-x64" ;; esac ;;
    *) echo "Unsupported OS '$os'. On Windows run under WSL/Git Bash, or install the win-x64 package manually." >&2 && exit 1 ;;
esac
[ -n "${rid:-}" ] || {
    echo "Unsupported architecture '$arch' on '$os'." >&2
    exit 1
}

install_dir="$DEST/$rid/$VERSION"
wrapper="$DEST/bin/roslyn-lsp"
if [ -f "$install_dir/.installed" ] && [ -x "$wrapper" ]; then
    echo "roslyn-lsp $VERSION ($rid) already installed"
    exit 0
fi

pkg="microsoft.codeanalysis.languageserver.$rid"
url="$FEED/$pkg/$VERSION/$pkg.$VERSION.nupkg"
tmp="$(mktemp -d)"
trap 'rm -rf "$tmp"' EXIT

echo "Downloading $pkg $VERSION ..."
curl -fsSL "$url" >"$tmp/pkg.nupkg"
unzip -q "$tmp/pkg.nupkg" "content/LanguageServer/$rid/*" -d "$tmp/x"

rm -rf "$install_dir"
mkdir -p "$install_dir"
cp -R "$tmp/x/content/LanguageServer/$rid/." "$install_dir/"

mkdir -p "$DEST/bin"
cat >"$wrapper" <<EOF
#!/usr/bin/env bash
exec dotnet "$install_dir/Microsoft.CodeAnalysis.LanguageServer.dll" "\$@"
EOF
chmod +x "$wrapper"
touch "$install_dir/.installed"
echo "Installed roslyn-lsp $VERSION ($rid) -> $wrapper"
