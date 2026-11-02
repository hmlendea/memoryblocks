# Data Model

This document describes the core data structures, relationships, and constraints.

## Entity Overview

```
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│   GameEngine    │──────►│    GameTable    │──────►│     Piece       │
│  (Session)      │  1:1  │   (Board)       │  1:N  │   (Tile)        │
└─────────────────┘       └─────────────────┘       └─────────────────┘
        │                         │                         │
        │                         │                         │
        ▼                         ▼                         ▼
  - tableSize               - Pieces[,]               - Value (int)
  - isRunning               - tableSize               - IsFlipped (bool)
  - gameTime                - piecesMaxValue          - IsCleared (bool)
  - flips                   - GenerateTable()         - PositionX (int)
  - piecesLeft              - InitializePieces()      - PositionY (int)
  - gdkWindowTable          - GetPiece(x,y)           - Size (int, legacy)
  - gdkWindowInfoBar
  - TimerTick()
  - FlipPiece()
  - CheckFlipped()
  - UnflipAllPieces()
  - DrawTable()
  - DrawInfoBar()
  - NewGame()
  - Completed (property)
```

## GameEngine (Session State)

### Fields

| Field | Type | Nullable | Default | Description |
|-------|------|----------|---------|-------------|
| `gameTable` | `GameTable` | No | `new GameTable(tableSize)` | Board model |
| `gdkWindowTable` | `Gdk.Window` | Yes | `null` | Drawing surface for board |
| `gdkWindowInfoBar` | `Gdk.Window` | Yes | `null` | Drawing surface for info bar |
| `tableSize` | `int` | No | Constructor arg | Grid dimension (4, 6, 8) |
| `isRunning` | `bool` | No | `true` | Game active |
| `gameTime` | `int` | No | `0` | Seconds elapsed |
| `flips` | `int` | No | `0` | Tiles currently flipped (0–2) |
| `piecesLeft` | `int` | No | `tableSize²/2` | Unmatched pairs remaining |
| `brBackground` | `Brush` | No | `GhostWhite` | Board background |
| `brUnflipped` | `Brush` | No | `CornflowerBlue` | Face-down tile |
| `brFlipped` | `Brush` | No | `LightBlue` | Face-up tile |
| `brInfoBackground` | `Brush` | No | `GhostWhite` | Info bar background |
| `brInfoText` | `Brush` | No | `Black` | Info bar text |
| `fontInfo` | `Font` | No | `Sans 14` | Info bar font |

### Properties

| Property | Type | Getter | Setter |
|----------|------|--------|--------|
| `Completed` | `bool` | `piecesLeft == 0` | — |

### Methods

| Method | Parameters | Returns | Side Effects |
|--------|------------|---------|--------------|
| `NewGame` | `int tableSize` | `void` | Full state reset |
| `FlipPiece` | `int x, int y` | `void` | Mutates `flips`, `piecesLeft`, `Piece.IsFlipped`, `Piece.IsCleared` |
| `CheckFlipped` | — | `void` | Mutates `flips`, `piecesLeft`, `Piece.IsFlipped`, `Piece.IsCleared` |
| `UnflipAllPieces` | — | `void` | Sets all `Piece.IsFlipped = false`, `flips = 0` |
| `DrawTable` | — | `void` | Renders to `gdkWindowTable` |
| `DrawInfoBar` | — | `void` | Renders to `gdkWindowInfoBar` |
| `TimerTick` | — | `bool` | Increments `gameTime`, calls `DrawInfoBar` |

## GameTable (Board Model)

### Fields

| Field | Type | Nullable | Default | Description |
|-------|------|----------|---------|-------------|
| `Pieces` | `Piece[,]` | No | `new Piece[tableSize, tableSize]` | 2D tile array |
| `tableSize` | `int` | No | Constructor arg | Grid dimension |
| `piecesMaxValue` | `int` | No | `102` | Max piece value (exclusive) |

### Methods

| Method | Parameters | Returns | Side Effects |
|--------|------------|---------|--------------|
| `GenerateTable` | — | `void` | Fills `Pieces` with random pairs |
| `InitializePieces` | — | `void` | Sets `PositionX/Y` on each piece |
| `GetPiece` | `int x, int y` | `Piece` | — |

### GenerateTable Algorithm

```csharp
// Creates pairs of values 0..(tableSize²/2 - 1), shuffled
int pairs = tableSize * tableSize / 2;
int[] values = new int[pairs * 2];
for (int i = 0; i < pairs; i++) {
    values[i * 2] = i;
    values[i * 2 + 1] = i;
}
// Fisher-Yates shuffle
for (int i = values.Length - 1; i > 0; i--) {
    int j = random.Next(i + 1);
    swap(values[i], values[j]);
}
// Fill row-major
for (int y = 0; y < tableSize; y++)
    for (int x = 0; x < tableSize; x++)
        Pieces[x, y].Value = values[y * tableSize + x];
```

**Critical Bug:** `piecesMaxValue = 102` but `pairs` max = 32 (for 8×8). Values only reach 31. However, if `tableSize` were larger or algorithm changed, values could exceed 36 (asset limit).

## Piece (Tile Model)

### Fields

| Field | Type | Nullable | Default | Description |
|-------|------|----------|---------|-------------|
| `Value` | `int` | No | `0` | Image index (0–101, effectively 0–31) |
| `IsFlipped` | `bool` | No | `false` | Face-up |
| `IsCleared` | `bool` | No | `false` | Matched and removed |
| `PositionX` | `int` | No | `0` | Column (set by `InitializePieces`) |
| `PositionY` | `int` | No | `0` | Row (set by `InitializePieces`) |
| `Size` | `int` | No | `48` | Legacy, unused |

### Constructor

```csharp
public Piece()
{
    Size = 48;  // Only initialisation
}
```

### State Constraints

| Constraint | Enforced By |
|------------|-------------|
| `IsCleared → !IsFlipped` | `GameEngine.CheckFlipped` sets both |
| `Value ≥ 0` | `GenerateTable` uses `i ≥ 0` |
| `Value < piecesMaxValue` | `GenerateTable` uses `i < pairs ≤ piecesMaxValue/2` |
| `PositionX ∈ [0, tableSize)` | `InitializePieces` |
| `PositionY ∈ [0, tableSize)` | `InitializePieces` |

## Data Relationships

### Ownership Hierarchy

```
GameEngine (owns)
    └── GameTable (owns)
            └── Piece[,] (owns each Piece)
```

### Reference Relationships

| From | To | Type | Cardinality |
|------|----|------|-------------|
| `GameEngine` | `GameTable` | Composition | 1:1 |
| `GameEngine` | `Gdk.Window` (table) | Reference | 1:1 (set in `NewGame`) |
| `GameEngine` | `Gdk.Window` (info) | Reference | 1:1 (set in `NewGame`) |
| `GameTable` | `Piece[,]` | Composition | 1:N (N = tableSize²) |
| `Piece` | (none) | — | — |

## Data Flow

### Initialisation
```
NewGame(6)
    │
    ├─► new GameTable(6)
    │       ├─► new Piece[6,6]
    │       ├─► GenerateTable() → random pairs 0–17
    │       └─► InitializePieces() → PositionX/Y
    │
    ├─► tableSize = 6
    ├─► isRunning = true
    ├─► gameTime = 0
    ├─► flips = 0
    ├─► piecesLeft = 18
    └─► Timer starts
```

### FlipPiece(x, y)
```
FlipPiece(x, y)
    │
    ├─► piece = gameTable.Pieces[x, y]
    │
    ├─► if (piece.IsCleared) return
    │
    ├─► if (piece.IsFlipped)
    │       piece.IsFlipped = false
    │       flips--
    │
    └─► else
            if (flips == 2) UnflipAllPieces()
            piece.IsFlipped = true
            flips++
            if (flips == 2) CheckFlipped()
```

### CheckFlipped()
```
CheckFlipped()
    │
    ├─► Find two pieces where IsFlipped == true
    │
    ├─► if (val1 == val2)
    │       Both: IsCleared = true, IsFlipped = false
    │       flips = 0
    │       piecesLeft--
    │
    └─► else
            (both remain IsFlipped = true, flips = 2)
```

## Serialisation (Not Implemented)

### Current
- No serialisation
- No persistence
- No DTOs

### If Added

```csharp
// DTOs for JSON serialisation
public class GameStateDto
{
    public int TableSize { get; set; }
    public int GameTime { get; set; }
    public int Flips { get; set; }
    public int PiecesLeft { get; set; }
    public bool IsRunning { get; set; }
    public PieceDto[,] Pieces { get; set; }
}

public class PieceDto
{
    public int Value { get; set; }
    public bool IsFlipped { get; set; }
    public bool IsCleared { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
}
```

## Data Validation Rules

| Rule | Location | Enforcement |
|------|----------|-------------|
| `tableSize` even, 4–8 | `MainWindow` menu handlers | Hardcoded values |
| `piecesLeft ≥ 0` | `GameEngine` | Decrement only in `CheckFlipped` |
| `flips ∈ [0, 2]` | `GameEngine` | Logic in `FlipPiece`, `UnflipAllPieces`, `CheckFlipped` |
| `Value < 37` (asset limit) | **NOT ENFORCED** | **BUG** — `piecesMaxValue = 102` |
| `PositionX/Y` valid | `GameTable.InitializePieces` | Loop bounds |

## Memory Layout

### Piece[,] Array
- Row-major: `Pieces[x, y]` where x=column, y=row
- Contiguous in memory (CLR multi-dimensional array)
- Size: `tableSize² × sizeof(Piece)` ≈ 64 × ~48 bytes = ~3 KB (max)

### Piece Object
- Reference type, allocated on heap
- Fields: 5 ints (20 bytes) + 2 bools (2 bytes, padded) + overhead
- ~48 bytes per instance

## Invariants (See invariants.md for Full List)

| Invariant | Description |
|-----------|-------------|
| `piecesLeft = count(IsCleared)/2` | Pairs cleared tracked correctly |
| `Completed ↔ piecesLeft == 0` | Win condition |
| `isRunning ↔ piecesLeft > 0` | Timer runs while game active |
| `flips ≤ 2` | Max two flipped |
| `IsCleared → !IsFlipped` | Cleared tiles never flipped |
| `∑ Value frequencies = 2` | Each value appears exactly twice |

## Extensibility Points

| Extension | Current | Effort |
|-----------|---------|--------|
| New piece types | Value int only | Low (add enum, sprite sheet) |
| Board shapes | Square only | Medium (change indexing) |
| Multiplayer | Single player | High (new architecture) |
| Persistence | None | Medium (add DTOs, serialisation) |
| Statistics | None | Low (add fields, serialise) |