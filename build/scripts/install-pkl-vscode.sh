#!/usr/bin/env bash
# Downloads and installs the Pkl VS Code extension from GitHub releases.
# The extension ships only as a VSIX (not in the VS Code marketplace) so it must be
# fetched from GitHub and installed with `code --install-extension`.
# Version is pinned via PKL_VSCODE_VERSION (see .config/mise.toml [env]).
set -euo pipefail

VERSION="${PKL_VSCODE_VERSION:?PKL_VSCODE_VERSION must be set (see .config/mise.toml [env])}"
ROOT="${MISE_PROJECT_ROOT:-$(git rev-parse --show-toplevel)}"
CACHE_DIR="$ROOT/.pkl-vscode"
VSIX="$CACHE_DIR/pkl-vscode-${VERSION}.vsix"
SENTINEL="$CACHE_DIR/.installed-${VERSION}"
URL="https://github.com/apple/pkl-vscode/releases/download/${VERSION}/pkl-vscode-${VERSION}.vsix"

if [[ -f "$SENTINEL" ]]; then
    echo "pkl-vscode ${VERSION} already installed"
    exit 0
fi

mkdir -p "$CACHE_DIR"
if [[ ! -f "$VSIX" ]]; then
    echo "Downloading pkl-vscode ${VERSION}..."
    curl -fsSL "$URL" -o "$VSIX"
fi

echo "Installing pkl-vscode ${VERSION}..."
code --install-extension "$VSIX"
touch "$SENTINEL"
echo "Installed pkl-vscode ${VERSION}"
