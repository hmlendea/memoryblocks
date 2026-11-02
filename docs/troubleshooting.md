# Troubleshooting

This document covers common issues, diagnostics, and resolutions.

## Startup Failures

### Application Won't Start

| Symptom | Cause | Resolution |
|---------|-------|------------|
| `DllNotFoundException: gtk` | GTK# not installed | `sudo apt-get install gtk-sharp2` |
| `DllNotFoundException: gdiplus` | libgdiplus missing | `sudo apt-get install libgdiplus` |
| `TypeInitializationException: Gtk.Application` | GTK runtime not found | Install GTK 2.x runtime |
| `PlatformNotSupportedException` | Wrong architecture | Run `mono --arch=32 MemoryBlocks.exe` |
| `EntryPointNotFoundException` | Mono/GTK version mismatch | Ensure Mono 4.5+, GTK# 2.12 |

### Blank/Black Window

| Symptom | Cause | Resolution |
|---------|-------|------------|
| Window opens but empty | Theme engine issue | `GTK_THEME=Adwaita mono MemoryBlocks.exe` |
| No drawing area visible | Widget not realised | Resize window, or check `daTable` in Stetic |
| Icons missing | Icon theme | Install `adwaita-icon-theme` |

### Permission Errors

| Symptom | Cause | Resolution |
|---------|-------|------------|
| `Access denied` on Linux | Executable bit missing | `chmod +x MemoryBlocks.exe` |
| Cannot write to directory | Running from read-only location | Copy to writable location |

## Runtime Errors

### Click Crashes Application

| Symptom | Cause | Resolution |
|---------|-------|------------|
| Crash on tile click | Click outside grid bounds | **Bug** — add bounds check in `OnDaTableButtonPressEvent` |
| Crash after many clicks | Resource exhaustion | **Bug** — cache Bitmaps, don't recreate per frame |

### Missing Piece Images

| Symptom | Cause | Resolution |
|---------|-------|------------|
| Exception in `DrawTable` | `piece.Value ≥ 37` but only 37 images | **Bug** — `piecesMaxValue=102` exceeds assets; clamp values to 36 |
| White/empty tiles | Resource load failed silently | Verify embedded resources: `monodis --resources MemoryBlocks.exe` |

### Timer Issues

| Symptom | Cause | Resolution |
|---------|-------|------------|
| Timer runs too fast/slow | `GLib.Timeout` not real-time | Acceptable for casual game |
| Timer stops after dialog | Modal dialog blocks main loop | Expected behaviour |
| Multiple timers after New Game | Old `GameEngine` not disposed | **Bug** — store timer ID, remove before new `GameEngine` |

## Build Failures

### xbuild/msbuild Errors

| Error | Cause | Resolution |
|-------|-------|------------|
| `Reference not found: gtk-sharp` | GTK# dev packages missing | `sudo apt-get install libgtk2.0-cil-dev` |
| `Could not find pkg-config` | pkg-config not installed | `sudo apt-get install pkg-config` |
| `Platform target x86 failed` | 32-bit libraries missing | `sudo apt-get install libc6-dev-i386` (Debian) |
| `Resource not found: gui.stetic` | Stetic file not embedded | Check `.csproj` EmbeddedResource includes |

### Mono Version Issues

| Error | Cause | Resolution |
|-------|-------|------------|
| `System.TypeLoadException` | Mono too old | Upgrade to Mono 6.12+ |
| `C# 7+ features not supported` | Old compiler | Use `msbuild` with newer Mono, or migrate to .NET Core |

## Performance Issues

### High CPU Usage

| Symptom | Cause | Resolution |
|---------|-------|------------|
| CPU spikes on resize | `DrawTable` called repeatedly | Throttle render with minimum interval |
| CPU high during game | Bitmap allocation per tile per frame | **Fix:** Cache `Bitmap` objects in `NewGame` |

### Memory Growth

| Symptom | Cause | Resolution |
|---------|-------|------------|
| Memory increases over time | Bitmaps not disposed | `using` block in `DrawTable` handles this |
| Memory leak on New Game | Old `GameEngine` not GC'd | Null references, force `GC.Collect()` if needed |

## Graphics Issues

### Flickering

| Symptom | Cause | Resolution |
|---------|-------|------------|
| Flicker on redraw | No double buffering | GTK DrawingArea should double-buffer; check `daTable.DoubleBuffered` |

### Wrong Colours

| Symptom | Cause | Resolution |
|---------|-------|------------|
| Colours don't match | `System.Drawing` vs GDK colour space | Known limitation of `gtk-dotnet` bridge |

## Diagnostic Commands

### Check Dependencies
```bash
# Linux: verify GTK#
pkg-config --modversion gtk-sharp-2.0
pkg-config --libs gtk-sharp-2.0

# Check Mono
mono --version

# Check libgdiplus
ldconfig -p | grep gdiplus
```

### Inspect Assembly
```bash
# List embedded resources
monodis --resources MemoryBlocks/bin/Release/MemoryBlocks.exe

# Check references
monodis --assemblyref MemoryBlocks/bin/Release/MemoryBlocks.exe

# Verify entry point
monodis --method=Main MemoryBlocks/bin/Release/MemoryBlocks.exe
```

### Runtime Diagnostics
```bash
# Enable GTK debug
GTK_DEBUG=interactive mono MemoryBlocks.exe

# Enable Mono debug
mono --debug MemoryBlocks.exe

# Trace GLib
G_DEBUG=gc-friendly mono MemoryBlocks.exe
```

### Log Analysis (When Logging Added)
```bash
# Check log file
tail -f ~/.local/share/MemoryBlocks/logs/memoryblocks-$(date +%Y%m%d).log

# Filter errors
grep ERROR ~/.local/share/MemoryBlocks/logs/memoryblocks-*.log
```

## Known Bugs (Workarounds)

| Bug | Workaround |
|-----|------------|
| Click outside grid crashes | Don't click outside the visible board |
| Values 37+ crash on render | Not reachable in normal play (max value 31) |
| Multiple timers after New Game | Restart application instead of New Game |
| Right-click does nothing | Use left-click only |
| No keyboard support | Mouse only |

## Debugging Tips

### Attach Debugger (MonoDevelop/VS Code)
1. Build Debug configuration
2. Set breakpoints in `GameEngine.FlipPiece`, `CheckFlipped`
3. Run with debugger attached

### Print Debugging (Temporary)
```csharp
// In GameEngine.FlipPiece
Console.WriteLine($"FlipPiece({x},{y}) flips={flips} piece.Value={piece.Value}");

// In TimerTick
Console.WriteLine($"TimerTick gameTime={gameTime} isRunning={isRunning}");
```

### Verify Board State
```csharp
// In CheckFlipped
for (int i = 0; i < tableSize; i++)
    for (int j = 0; j < tableSize; j++)
        Console.WriteLine($"Piece[{i},{j}] Value={Pieces[i,j].Value} Flipped={Pieces[i,j].IsFlipped} Cleared={Pieces[i,j].IsCleared}");
```

## Escalation

If issue not resolved:
1. Check [GitHub Issues](https://github.com/.../issues) for known problems
2. Create new issue with:
   - OS, Mono version, GTK# version
   - Full exception stack trace
   - Steps to reproduce
   - Expected vs actual behaviour