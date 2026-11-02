# Quick Start

This document provides the fastest path to building and running Memory Blocks.

## Prerequisites

### Linux (Primary Target)
```bash
# Ubuntu/Debian
sudo apt-get install mono-complete gtk-sharp2 libgdiplus

# Fedora
sudo dnf install mono-complete gtk-sharp2 libgdiplus

# Arch
sudo pacman -S mono gtk-sharp-2 libgdiplus
```

### Windows
1. Install [.NET Framework 4.5+](https://dotnet.microsoft.com/download/dotnet-framework)
2. Install [GTK# for .NET 2.12](https://github.com/GtkSharp/GtkSharp/releases)

### macOS
```bash
brew install mono gtk+ gtk-sharp
# May need XQuartz: brew install --cask xquartz
```

## Build

```bash
# Clone
git clone <repository-url>
cd memoryblocks

# Build Release
xbuild MemoryBlocks.sln /p:Configuration=Release

# Or Debug
xbuild MemoryBlocks.sln /p:Configuration=Debug
```

## Run

### Linux/macOS
```bash
mono MemoryBlocks/bin/Release/MemoryBlocks.exe
```

### Windows
```cmd
MemoryBlocks\bin\Release\MemoryBlocks.exe
```

## Play

1. **Start** — Application opens with 6×6 board (Moderate)
2. **Click tiles** — Left-click to flip, find matching pairs
3. **Menu → Game** — Choose Easy (4×4), Moderate (6×6), Hard (8×8)
4. **Complete** — Dialog appears when all pairs matched
5. **Menu → Game → Exit** — Quit

## Controls

| Action | Input |
|--------|-------|
| Flip tile | Left mouse click |
| New game (Easy) | Game → Easy |
| New game (Moderate) | Game → Moderate |
| New game (Hard) | Game → Hard |
| Quit | Game → Exit |

## Verify Installation

```bash
# Check build output
ls -la MemoryBlocks/bin/Release/MemoryBlocks.exe

# Quick smoke test (Linux)
timeout 3 mono MemoryBlocks/bin/Release/MemoryBlocks.exe || true
echo "Launch test passed"
```

## Common Issues

| Issue | Fix |
|-------|-----|
| `DllNotFoundException: gtk` | Install `gtk-sharp2` / `libgtk2.0-0` |
| `DllNotFoundException: gdiplus` | Install `libgdiplus` |
| `PlatformNotSupportedException` | Ensure x86 build on 64-bit Linux: `mono --arch=32` |
| Blank window | Ensure GTK theme installed, try `GTK_THEME=Adwaita` |
| Missing piece images | Verify 37 PNGs in `Resources/Pieces/` embedded |

## Development Setup

### MonoDevelop / Visual Studio for Mac
1. Open `MemoryBlocks.sln`
2. Set configuration to `Release|x86`
3. Press `F5` to run

### VS Code
1. Install C# extension
2. Install Mono Debug extension
3. Create `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "MemoryBlocks",
      "type": "mono",
      "request": "launch",
      "program": "${workspaceFolder}/MemoryBlocks/bin/Release/MemoryBlocks.exe",
      "cwd": "${workspaceFolder}/MemoryBlocks/bin/Release"
    }
  ]
}
```

## Package Distribution

```bash
# Linux package
tar -czf MemoryBlocks-linux.tar.gz -C MemoryBlocks/bin/Release .

# Windows package
zip -r MemoryBlocks-windows.zip MemoryBlocks/bin/Release/
```

## Next Steps

- Read [Architecture](architecture.md) for system overview
- Read [Components](components/) for code structure
- Read [Build and Deployment](build-and-deployment.md) for packaging details
- See [Troubleshooting](troubleshooting.md) for common problems