# State and Persistence

This document describes state management, persistence, and data flow.

## State Overview

Memory Blocks is a **stateless application** with respect to persistence. All state is held in-memory for the duration of a single process lifetime. No data is written to disk, no configuration is saved, no high scores are recorded, and no game progress is restored across sessions.

## In-Memory State

### Application State (Program.cs → MainWindow)

| State | Location | Lifetime |
|-------|----------|----------|
| GTK Application context | `Application` static | Process |
| Main window instance | `MainWindow` | Process |
| UI widget tree | `MainWindow` + Stetic | Process |

### Game Session State (GameEngine)

| Field | Type | Description | Mutability |
|-------|------|-------------|------------|
| `gameTable` | `GameTable` | Board model | Immutable reference, mutable contents |
| `gdkWindowTable` | `Gdk.Window` | Drawing surface for board | Set once in `NewGame` |
| `gdkWindowInfoBar` | `Gdk.Window` | Drawing surface for info bar | Set once in `NewGame` |
| `tableSize` | `int` | Grid dimension (4, 6, or 8) | Set in `NewGame`, then immutable |
| `isRunning` | `bool` | Game active flag | Mutable |
| `gameTime` | `int` | Seconds elapsed | Mutable (timer) |
| `flips` | `int` | Tiles currently flipped (0–2) | Mutable |
| `piecesLeft` | `int` | Unmatched pairs remaining | Mutable |
| `brushes/fonts` | `Brush`, `Font` | Rendering resources | Created in `NewGame`, disposed implicitly |

### Board State (GameTable)

| Field | Type | Description |
|-------|------|-------------|
| `Pieces` | `Piece[,]` | 2D array of tiles, size `tableSize × tableSize` |
| `tableSize` | `int` | Grid dimension |
| `piecesMaxValue` | `int` | Maximum piece value (102, but only 37 assets) |

### Tile State (Piece)

| Field | Type | Description | Transitions |
|-------|------|-------------|-------------|
| `Value` | `int` | Image index (0–101, effectively 0–36) | Set once in `InitializePieces` |
| `IsFlipped` | `bool` | Face-up | `false → true` (flip), `true → false` (unflip/match) |
| `IsCleared` | `bool` | Matched and removed | `false → true` (match), never reverts |
| `PositionX` | `int` | Column | Set once |
| `PositionY` | `int` | Row | Set once |
| `Size` | `int` | Legacy (48) | Unused |

## State Transitions

### Game Lifecycle

```
Process Start
    │
    ├─► Application.Init()
    ├─► MainWindow() → NewGame(6)
    │       ├─► GameEngine(6)
    │       │     ├─► GameTable(6)
    │       │     │     └─► Piece[6,6] with random pairs
    │       │     ├─► isRunning = true
    │       │     ├─► gameTime = 0
    │       │     ├─► flips = 0
    │       │     ├─► piecesLeft = 18
    │       │     └─► Timer starts (1000ms)
    │       └─► UI realised, DrawTable/DrawInfoBar
    │
    ├─► Application.Run() — GTK main loop
    │       │
    │       ├─► User clicks → FlipPiece → state mutations
    │       ├─► Timer ticks → gameTime++, DrawInfoBar
    │       └─► Match → piecesLeft--, check Completed
    │
    ├─► piecesLeft == 0 → Completed = true → Dialog
    │       └─► isRunning = false (timer stops)
    │
    ├─► User clicks "New Game" menu → NewGame(size)
    │       └─► Full state reset (new GameEngine, new GameTable)
    │
    └─► User closes window → Application.Quit() → Process Exit
```

### Tile State Machine

```
┌─────────────┐
│  HIDDEN     │  (IsFlipped=false, IsCleared=false)
│  (face-down)│
└──────┬──────┘
       │ FlipPiece (click)
       ▼
┌─────────────┐
│  FLIPPED    │  (IsFlipped=true, IsCleared=false)
│  (face-up)  │
└──────┬──────┘
       │
       ├─► Unflip (click same tile) ──────────────────┐
       │                                               │
       ├─► Match (CheckFlipped finds pair)            │
       │       │                                       │
       │       ▼                                       │
       │  ┌─────────────┐                              │
       │  │  CLEARED    │  (IsFlipped=false,           │
       │  │  (matched)  │   IsCleared=true)            │
       │  └─────────────┘                              │
       │       │                                       │
       │       └─► (terminal — never changes)          │
       │                                               │
       └─► No match (flips=2, next click)              │
               │                                       │
               ▼                                       │
        ┌─────────────┐                                │
        │  FLIPPED    │  (stays face-up until          │
        │  (waiting)  │   next click triggers          │
        └─────────────┘   UnflipAllPieces)             │
               │                                       │
               └───────────────────────────────────────┘
```

## Persistence (Absent)

### What Is NOT Persisted

| Data | Persisted? | Location if Yes |
|------|------------|-----------------|
| High scores | ❌ No | — |
| Last grid size | ❌ No | — |
| Window position/size | ❌ No | — |
| Game in progress | ❌ No | — |
| Statistics (games played, win rate) | ❌ No | — |
| User preferences | ❌ No | — |
| Accessibility settings | ❌ No | — |

### What IS Persisted (Build-Time Only)

| Data | Location |
|------|----------|
| Piece images (37 PNGs) | Embedded resources in assembly |
| UI definition (gui.stetic) | Embedded resource |
| Application icon | Embedded resource |
| Compiled code | Assembly (`MemoryBlocks.exe`) |

## State Serialisation (Not Implemented)

If persistence were added, recommended approach:

### Save Game State
```csharp
public class GameState
{
    public int TableSize { get; set; }
    public int GameTime { get; set; }
    public int Flips { get; set; }
    public int PiecesLeft { get; set; }
    public PieceState[,] Pieces { get; set; }
    public bool IsRunning { get; set; }
}

public class PieceState
{
    public int Value { get; set; }
    public bool IsFlipped { get; set; }
    public bool IsCleared { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
}
```

### Save Location
- Linux: `$XDG_DATA_HOME/MemoryBlocks/savegame.json` or `~/.local/share/MemoryBlocks/savegame.json`
- Windows: `%LOCALAPPDATA%\MemoryBlocks\savegame.json`

### Auto-Save Triggers
- After each successful match
- On window close (if game in progress)
- On difficulty change (optional)

### Load on Startup
- Check for save file in `MainWindow` constructor
- Offer "Continue Game" menu option if valid save exists
- Validate save version compatibility

## State Invariants

| Invariant | Enforced By |
|-----------|-------------|
| `0 ≤ flips ≤ 2` | `FlipPiece`, `UnflipAllPieces`, `CheckFlipped` |
| `piecesLeft ≥ 0` | `CheckFlipped` (decrement only on match) |
| `IsCleared → !IsFlipped` | `CheckFlipped` (sets both) |
| `piecesLeft == 0 ↔ Completed` | `Completed` property getter |
| `isRunning ↔ piecesLeft > 0` | `TimerTick`, `NewGame`, `CheckFlipped` |
| `∑(IsCleared) / 2 + piecesLeft == tableSize² / 2` | `GameTable.GenerateTable` + `CheckFlipped` |
| `Value ∈ [0, piecesMaxValue)` | `GameTable.GenerateTable` (but assets only 0–36) |

## State Debugging

### Current Debugging Aids
- None — no logging, no debug overlay, no state inspector

### Recommended Debug Features
1. **Debug overlay** — Press `F12` to show `piecesLeft`, `flips`, `gameTime`, tile values
2. **State dump** — `Logger.Debug(JsonSerializer.Serialize(gameEngine))` on key events
3. **Deterministic seed** — Allow fixed RNG seed for reproducible games
4. **Replay system** — Record click sequence, replay for debugging

## Concurrency and State

- **Single-threaded** — All state mutations on GTK main thread
- **No locks needed** — No background threads access game state
- **Timer callback** — `GLib.Timeout` runs on main loop, same thread
- **Event handlers** — GTK signals delivered on main thread

## State Reset

### NewGame(size) — Full Reset
```csharp
public void NewGame(int tableSize)
{
    gameTable = new GameTable(tableSize);  // New board, new pieces
    this.tableSize = tableSize;
    isRunning = true;
    gameTime = 0;
    flips = 0;
    piecesLeft = (tableSize * tableSize) / 2;
    // Brushes, fonts recreated
    // Timer continues (same handler)
    DrawTable();
    DrawInfoBar();
}
```

### Menu Handlers
- **Easy** → `NewGame(4)`
- **Moderate** → `NewGame(6)`
- **Hard** → `NewGame(8)`

## Data Flow Summary

```
User Input (click)
    │
    ▼
MainWindow.OnDaTableButtonPressEvent
    │
    ▼
GameEngine.FlipPiece(x, y)
    │
    ├─► Mutates: Piece.IsFlipped, flips, piecesLeft
    │
    ▼
GameEngine.CheckFlipped() / UnflipAllPieces()
    │
    ▼
GameEngine.DrawTable() → Reads: Piece.Value, IsFlipped, IsCleared
    │
    ▼
GameEngine.DrawInfoBar() → Reads: piecesLeft, gameTime
    │
    ▼
GTK Main Loop → Screen Update
```

No external data flows. Pure in-memory, single-process, single-threaded.