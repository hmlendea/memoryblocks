# Repository Overview

This document describes the repository purpose, scope, and entry points.

## Purpose

Memory Blocks is a local desktop tile-matching puzzle game implemented in C# targeting Mono and GTK# 2.12. The game presents a grid of face-down tiles; players flip two tiles per turn, attempting to find matching pairs. The goal is to clear the board by matching all pairs.

**Scope:**
- Single-player desktop puzzle game
- Local execution only — no network, no accounts, no cloud
- Linux primary target (Mono + GTK# 2.12)
- Windows build possible via Mono or .NET Framework

**Out of Scope:**
- Multiplayer, online features, leaderboards
- Mobile platforms
- Web deployment
- User accounts, profiles, progression
- High scores, statistics, persistence
- Automated testing infrastructure

## Entry Points

### Application Entry Point

**File:** `MemoryBlocks/Program.cs`
**Class:** `MemoryBlocks.MainClass`
**Method:** `static void Main(string[] args)`

```csharp
public static void Main(string[] args)
{
    Application.Init();           // Initialise GTK#
    MainWindow win = new MainWindow();
    win.Show();                   // Show main window
    Application.Run();            // Start GTK main loop
}
```

**Responsibilities:**
1. Initialise GTK# application context
2. Construct main window (triggers UI build, engine creation, first game)
3. Start GTK event loop (blocks until `Application.Quit()`)

### Window Construction Entry Point

**File:** `MemoryBlocks/MainWindow.cs`
**Class:** `MemoryBlocks.MainWindow` (partial)
**Constructor:** `public MainWindow() : base(Gtk.WindowType.Toplevel)`

**Responsibilities:**
1. Call `Build()` — Stetic-generated UI construction
2. Configure drawing areas (`daTable.DoubleBuffered = false`)
3. Instantiate `GameEngine` with default 6×6 grid
4. Bind `Gdk.Window` references from drawing areas to engine
5. Start first game via `game.NewGame(6)`
6. Subscribe to `ExposeEvent` on both drawing areas
7. Enable `ButtonPressMask` on game table drawing area
8. Wire menu action handlers (difficulty, retry, about)

### Game Session Entry Point

**File:** `MemoryBlocks/Game/GameEngine.cs`
**Class:** `MemoryBlocks.Game.GameEngine`
**Method:** `public void NewGame(int tableSize)`

**Responsibilities:**
1. Create new `GameTable(tableSize)` — generates randomised board
2. Store `tableSize`, reset `tileSpacing = 2`
3. Reset counters: `gameTime = 0`, `flips = 0`, `piecesLeft = (tableSize * tableSize) / 2`
4. Set `isRunning = true`
5. Trigger initial render: `DrawTable()`, `DrawInfoBar()`

### Timer Entry Point

**File:** `MemoryBlocks/Game/GameEngine.cs`
**Constructor:** `public GameEngine(int tableSize)`

**Registration:**
```csharp
GLib.Timeout.Add(1000, new GLib.TimeoutHandler(TimerTick));
```

**Callback:** `private bool TimerTick()`
- Executes every ~1000ms on GTK main loop
- Increments `gameTime` if `isRunning`
- Calls `DrawInfoBar()` to update timer display
- Stops timer when `piecesLeft == 0` (sets `isRunning = false`)
- Returns `true` to continue timeout

### User Input Entry Point

**File:** `MemoryBlocks/MainWindow.cs`
**Method:** `protected void OnDaTableButtonPressEvent(object o, ButtonPressEventArgs args)`

**Trigger:** Left mouse click on `daTable` drawing area

**Flow:**
1. Calculate tile coordinates: `x = Event.X / tileSize`, `y = Event.Y / tileSize`
2. Call `game.FlipPiece(x, y)` if left button (button 1)
3. Call `game.DrawTable()`, `game.DrawInfoBar()`
4. Check `game.Completed` → show completion dialog

## Key Types

| Type | Location | Role |
|------|----------|------|
| `MainClass` | `Program.cs` | Application bootstrap |
| `MainWindow` | `MainWindow.cs` | Top-level window, UI event hub |
| `GameEngine` | `Game/GameEngine.cs` | Session orchestration, rules, rendering |
| `GameTable` | `Game/GameTable.cs` | Board data structure, pair generation |
| `Piece` | `Game/Piece.cs` | Individual tile data model |

## Execution Model

- **Single-threaded:** All code runs on GTK main loop thread
- **Event-driven:** User input → event handlers → engine methods → rendering
- **Timer-driven:** `GLib.Timeout` callback → state update → rendering
- **No background threads:** No `Task`, `Thread`, `BackgroundWorker`, or thread pool usage

## Configuration Entry Points

| Setting | Source | Applied At |
|---------|--------|------------|
| Grid Size / Difficulty | Menu actions (`OnActEasyActivated`, etc.) | `game.NewGame(size)` |
| Board Colors | `GameEngine` constructor defaults | Engine instantiation |
| Tile Spacing | `GameEngine.NewGame()` hardcoded | New game start |
| Timer Interval | `GLib.Timeout.Add(1000, ...)` | Engine construction |

## Build Entry Points

| Command | Purpose |
|---------|---------|
| `xbuild MemoryBlocks.sln` | Build solution (Mono) |
| `msbuild MemoryBlocks.sln` | Build solution (Windows/.NET Framework) |
| `mono MemoryBlocks/bin/Debug/MemoryBlocks.exe` | Run debug build |
| `mono MemoryBlocks/bin/Release/MemoryBlocks.exe` | Run release build |

## Packaging Entry Points

**File:** `Packages.mdproj`
- **Linux:** `MemoryBlocks-Linux-x86.tar.gz` (Release|x86)
- **Windows:** `MemoryBlocks-Windows-x86.zip` (Debug|x86)

## Repository Conventions

- **Language:** C# (.NET Framework 4.5 / Mono)
- **Indentation:** 4 spaces (no tabs)
- **Naming:** PascalCase for types/members, camelCase for private fields
- **File per Type:** One class per identically named file
- **Partial Classes:** `MainWindow` split across `MainWindow.cs` and `gtk-gui/MemoryBlocks.MainWindow.cs`
- **Embedded Resources:** Assets compiled into assembly
- **No Tests:** Manual verification only