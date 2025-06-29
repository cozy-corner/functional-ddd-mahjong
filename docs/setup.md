# Setup Guide

## .NET 8 SDK Installation for macOS

This guide covers the installation of .NET 8 SDK required for F# development.

### Installation using Homebrew

1. Add the tap for specific .NET SDK versions:
```bash
brew tap isen-ng/dotnet-sdk-versions
```

2. Install .NET 8.0.400:
```bash
brew install --cask dotnet-sdk8-0-400
```

### Configure PATH

Add .NET to your PATH by adding this line to `~/.zshrc`:
```bash
export PATH="/usr/local/share/dotnet:$PATH"
```

Reload your shell configuration:
```bash
source ~/.zshrc
```

### Verify Installation

Check that .NET SDK is properly installed:
```bash
dotnet --version
```

Expected output: `8.0.411` or similar 8.0.x version.