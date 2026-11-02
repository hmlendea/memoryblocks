# Architecture

This document elaborates on the high-level architecture described in the root [ARCHITECTURE.md](../ARCHITECTURE.md), providing deeper implementation detail for each architectural area.

## Architectural Style

Memory Blocks follows an event-driven Model-View-Presenter (MVP) / Model-View-Controller (MVC) architectural style suitable for GTK-based desktop applications.

```
┌─────────────────────────────────────────────────────────────────┐
│                        View Layer                               │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │ MainWindow  │  │  daTable    │  │ daInfoBar   │             │
│  │ (Gtk.Window)│  │(DrawingArea)│  │(DrawingArea)│             │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘             │
└─────────┼────────────────┼────────────────┼────────────────────┘
          │                │                │
          ▼                ▼                ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Game Domain Engine                          │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    GameEngine                           │   │
│  │  • Session state (timer, score, flips, pieces left)    │   │
│  │  • Game rule validation (flip logic, match checking)   │   │
│  │  • GDI+ rendering via Gtk.DotNet.Graphics              │   │
│  └──────────────────────────┬──────────────────────────────┘   │
│                             │                                   │
│                             ▼                                   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                    GameTable                            │   │
│  │  • Grid geometry (Piece[,] matrix)                      │   │
│  │  • Random pair distribution algorithm                   │   │
│  └──────────────────────────┬──────────────────────────────┘   │
│                             │                                   │
│                             ▼                                   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │                      Piece                              │   │
│  │  • Value (0-102)                                        │   │
│  │  • IsFlipped, IsCleared                                 │   │
│  │  • PositionX, PositionY                                 │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## Component Boundaries

### View Layer (`MemoryBlocks/MainWindow.cs`, `MemoryBlocks/gtk-gui/`)

**Responsibilities:**
- GTK application lifecycle management (`Application.Init()`, `Application.Run()`)
- Window widget hierarchy construction (menu bar, drawing areas)
- User input translation (pixel coordinates → grid indices)
- Menu action dispatching (difficulty levels, retry, about dialog)
- Modal dialog display

**Ownership:** Owned by `MainClass`; lifetime = application runtime

**Dependencies:** `Gtk.Window`, `GameEngine`, `Gtk.DrawingArea`

**Boundary Rule:** Delegates all game state logic and tile verification to `GameEngine`. Does not contain game rules.

### Game Engine (`MemoryBlocks/Game/GameEngine.cs`)

**Responsibilities:**
- Session state management (timer, score, flips count, pieces left, completed status)
- Game rule validation (flip logic, match checking, unflip logic)
- GDI+ rendering onto GDK drawing surfaces via `Gtk.DotNet.Graphics`
- Timer tick callback via `GLib.Timeout.Add`

**Ownership:** Owned by `MainWindow`; created once per application lifetime

**Dependencies:** `GameTable`, `Gdk.Window`, `Gtk.DotNet.Graphics`, `System.Drawing`, `System.Timers`

**Boundary Rule:** Operates on generic `Gdk.Window` references; no dependency on specific GTK widget types or window layout.

### Game Domain Model (`MemoryBlocks/Game/GameTable.cs`, `MemoryBlocks/Game/Piece.cs`)

**GameTable Responsibilities:**
- Tile board data structure (`Piece[,]` matrix)
- Random matching pair generation and placement algorithm
- Table regeneration for new games

**Piece Responsibilities:**
- Individual tile data model (value, flipped state, cleared state, position)

**Ownership:** `GameTable` created per game session by `GameEngine`; `Piece` objects owned by `GameTable` matrix

**Dependencies:** `System.Random`, `System.Collections.Generic.List`

**Boundary Rule:** No dependency on GTK#, GDK, or System.Drawing. Pure domain logic.

## Runtime Flow

### Application Startup

```
MainClass.Main()
  ├─ Application.Init()                    // GTK# initialisation
  ├─ new MainWindow()                      // UI construction
  │    ├─ Build()                          // Stetic-generated UI
  │    ├─ new GameEngine(6)                // Engine with 6x6 default
  │    ├─ game.GdkWindowTable = daTable.GdkWindow
  │    ├─ game.GdkWindowInfoBar = daInfoBar.GdkWindow
  │    ├─ game.NewGame(6)                  // First game session
  │    │    ├─ new GameTable(6)            // Domain model
  │    │    │    ├─ InitializePieces()     // Piece[,] allocation
  │    │    │    └─ GenerateTable()        // Random pair placement
  │    │    ├─ Reset counters (time, flips, piecesLeft)
  │    │    ├─ DrawTable()                 // Initial render
  │    │    └─ DrawInfoBar()               // Initial info bar
  │    ├─ daTable.ExposeEvent += DrawTable
  │    ├─ daInfoBar.ExposeEvent += DrawInfoBar
  │    └─ daTable.AddEvents(ButtonPressMask)
  ├─ win.Show()
  └─ Application.Run()                     // GTK main loop
```

### Timer Tick

```
GLib.Timeout.Add(1000, TimerTick)          // Registered in GameEngine ctor
  └─ TimerTick() (every 1000ms on main loop)
       ├─ if (isRunning)
       │    ├─ if (piecesLeft == 0) → isRunning = false
       │    ├─ gameTime += 1
       │    └─ DrawInfoBar()
       └─ return true                       // Continue timeout
```

## Data Architecture

### In-Memory State

| Data | Owner | Representation | Lifecycle |
|------|-------|----------------|-----------|
| Game Board | `GameTable` | `Piece[,]` (2D array) | Re-allocated on `NewGame` |
| Tile Properties | `Piece` | Object fields (`Value`, `IsFlipped`, `IsCleared`, `PositionX`, `PositionY`) | Mutable during session |
| Session Counters | `GameEngine` | `int gameTime`, `int flips`, `int piecesLeft`, `bool isRunning` | Reset on `NewGame` |
| Embedded Assets | Assembly | 37 PNG resources (`piece0.png`–`piece36.png`) | Read-only, loaded at render time |

### Piece State Machine

```
[Unflipped, Not Cleared] ──click──► [Flipped, Not Cleared]
       ▲                                    │
       │                                    │ (match)
       │                                    ▼
       │                              [Cleared] (terminal)
       │                                    │
       │                         (no match, next click)
       └────────────────────────────────────┘
```

## Interfaces and Integrations

| Interface | Direction | Contract | Owner | Failure Semantics |
|-----------|-----------|----------|-------|-------------------|
| GTK# Event Loop | Inbound/Bidirectional | C# delegates, `Gdk.EventMask.ButtonPressMask`, `ExposeEvent` | `MainWindow` | Unhandled exceptions terminate process |
| Gtk.DotNet Graphics | Outbound | `Gtk.DotNet.Graphics.FromDrawable(Gdk.Window)` → `System.Drawing.Graphics` | `GameEngine` | Surface drawing errors degrade visually |
| Embedded Resources | Outbound | `Gdk.Pixbuf.LoadFromResource("MemoryBlocks.Resources.Pieces.piece{ID}.png")` | `GameEngine.DrawTable()` | Missing resource → exception at render time |

## Dependency Direction

```
Program.cs
    └─ MainWindow.cs
          ├─ GameEngine.cs
          │    ├─ GameTable.cs
          │    │    └─ Piece.cs
          │    ├─ Gdk.Window (via Gtk.DotNet)
          │    └─ System.Drawing.Graphics
          └─ GTK# Widgets (Window, DrawingArea, Menu, Dialog)
```

**Rules:**
- High-level windowing (`MainWindow`) depends on game domain (`GameEngine`)
- Domain entities (`GameTable`, `Piece`) have zero GTK#/GUI dependencies
- `GameEngine` interacts with GDK purely through `Gdk.Window` references

## External Dependencies

| Dependency | Version | Purpose | Integration |
|------------|---------|---------|-------------|
| Mono Runtime / .NET Framework | 4.5 | Execution environment | System runtime |
| GTK# (`gtk-sharp`, `gdk-sharp`, `glib-sharp`, `pango-sharp`, `atk-sharp`, `glade-sharp`) | 2.12 | Windowing, widgets, main loop | Native GTK2 binding |
| `gtk-dotnet` | 2.12 | GDK ↔ System.Drawing bridge | `Gtk.DotNet.Graphics.FromDrawable` |
| `System.Drawing` | 4.0 | GDI+ rendering API | Drawing operations in `GameEngine` |
| `Mono.Posix` | 4.0 | POSIX interop | Platform integration |

## Deployment and Operations

| Concern | Design |
|---------|--------|
| Deployment Model | Standalone executable (`MemoryBlocks.exe`) |
| Runtime Requirements | Mono + GTK# 2.12 installed on target |
| Process Topology | Single GUI process, main thread only |
| Persistence | Purely in-memory; no disk I/O |
| Packaging | `Packages.mdproj` defines Linux tar.gz and Windows zip builders |

## Compatibility Contracts

| Contract | Invariant | Verification |
|----------|-----------|--------------|
| Target Framework | .NET Framework 4.5 / Mono profile | Build via MonoDevelop / xbuild |
| Resource Naming | `MemoryBlocks.Resources.Pieces.piece{ID}.png` | Runtime resource loading |
| Grid Sizes | 4×4 (Easy), 6×6 (Moderate), 8×8 (Hard) | Menu actions → `NewGame(size)` |

## Design Constraints

1. **GTK2 / GTK# 2.12 Binding:** Built specifically around GTK2 and Mono's GTK# bindings. Migration to GTK3/GTK4 would require significant UI layer changes.
2. **Single-Threaded GUI:** All execution driven by GTK main loop; no background threads.
3. **GDI+ on GTK:** Rendering uses `System.Drawing` via `gtk-dotnet` bridge; not native Cairo.
4. **Fixed Asset Set:** 37 piece images (0–36); `GameTable` uses `piecesMaxValue = 102` but only 37 unique images exist.
5. **No Automated Tests:** Verification is manual only.