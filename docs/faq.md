# Frequently Asked Questions

## General

### What is Memory Blocks?
A classic memory matching game (Concentration) built with C# and GTK#. Flip tiles to find matching pairs.

### What platforms does it support?
Primary: Linux (Mono + GTK# 2.12). Also runs on Windows (.NET Framework 4.5 + GTK#) and macOS (Mono + GTK#).

### Is it open source?
Yes, licensed under GPL-3.0. See `LICENSE` file.

### Where is the source code?
In the `MemoryBlocks/` directory. Main entry point: `Program.cs`.

## Gameplay

### How do I play?
Click tiles to flip them. Find all matching pairs. The timer tracks your time.

### What are the difficulty levels?
- **Easy:** 4×4 grid (8 pairs)
- **Moderate:** 6×6 grid (18 pairs) — default
- **Hard:** 8×8 grid (32 pairs)

### Can I use the keyboard?
No, mouse only. Menu navigation works with keyboard (Alt+G for Game menu).

### Is there a high score table?
No. No persistence of any kind.

### Can I save my game?
No. Closing the window loses progress.

### Why are there only 37 different tile images?
That's all that were created. The code supports up to 102 values but only 37 images exist (piece0.png–piece36.png).

## Technical

### Why GTK# 2.12?
Legacy choice. Stetic designer (visual UI builder) only works with GTK2. Modern GTK3/4 would require rewrite.

### Why .NET Framework 4.5 / Mono?
Targeted at Linux/Mono ecosystem circa 2010s. Not migrated to modern .NET.

### Why x86 only?
`gtk-dotnet` bridge (GDK ↔ System.Drawing) has 32-bit dependencies on some distributions.

### What is `gtk-dotnet`?
A bridge library that lets `System.Drawing.Graphics` draw on GDK drawables. Used for rendering.

### Why `System.Drawing` on Linux?
Mono's implementation uses libgdiplus (Cairo-based). Works but not hardware-accelerated.

### Can I build with Visual Studio on Windows?
Yes, open `MemoryBlocks.sln` in Visual Studio 2019/2022 with .NET Framework 4.5 targeting pack.

### Can I build with VS Code?
Yes, with C# extension and Mono Debug extension. See [Quick Start](quick-start.md#development-setup).

## Troubleshooting

### "DllNotFoundException: gtk"
Install GTK# 2.12: `sudo apt-get install gtk-sharp2` (Ubuntu/Debian).

### "DllNotFoundException: gdiplus"
Install libgdiplus: `sudo apt-get install libgdiplus`.

### Application crashes when I click
Don't click outside the visible board. Known bug: no bounds checking.

### Timer seems inaccurate
`GLib.Timeout` is approximate. Not a real-time clock.

### "New Game" creates multiple timers
Known bug. Restart the application instead of using New Game menu.

## Development

### How do I add new tile images?
1. Add PNG to `MemoryBlocks/Resources/Pieces/` named `piece{N}.png`
2. Include in `.csproj` as EmbeddedResource
3. Update `GameTable.piecesMaxValue` if exceeding current max

### How do I change colours?
Edit `GameEngine.NewGame()` — `brBackground`, `brUnflipped`, `brFlipped`, etc.

### How do I add a new difficulty?
Add menu item in Stetic, handler in `MainWindow`, call `NewGame(size)`.

### Where is the UI defined?
`MemoryBlocks/gtk-gui/gui.stetic` (Stetic designer format). Partial code in `generated.cs` and `MemoryBlocks.MainWindow.cs`.

### Can I run tests?
No test project exists yet. See [Testing](testing.md) for recommended setup.

### How do I contribute?
See [CONTRIBUTING.md](../CONTRIBUTING.md).

## Licensing

### Can I use this in my project?
Yes, under GPL-3.0 terms. You must provide source and license your derivative work under GPL-3.0.

### Can I sell this?
Yes, but you must provide source code to buyers under GPL-3.0.

### What about the tile images?
Part of the repository, same GPL-3.0 license.

## Support

### Where do I report bugs?
GitHub Issues (if repository hosted there).

### Is there a mailing list?
No.

### Can I get help?
Check [Troubleshooting](troubleshooting.md) first, then file an issue.