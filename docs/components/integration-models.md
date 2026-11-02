# Integration Models — GameTable and Piece

This document describes the domain models: `GameTable` (board data structure, pair generation) and `Piece` (tile data model).

## GameTable

**File:** `MemoryBlocks/Game/GameTable.cs`
**Namespace:** `MemoryBlocks.Game`
**Class:** `GameTable`

### Purpose

Represents the game board geometry and the random pair distribution algorithm. Pure domain logic — no GTK#, GDK, or System.Drawing dependencies.

### Public API

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `TableSize` | `int` | Grid dimension (read-only) |
| `Pieces` | `Piece[,]` | 2D array of tiles (read/write) |

#### Methods

| Method | Description |
|--------|-------------|
| `GameTable(int tableSize)` | Constructor — allocates pieces, generates randomised board |
| `Regenerate()` | Clears and regenerates board with same size |

### Internal State

| Field | Type | Description |
|-------|------|-------------|
| `pieces` | `Piece[,]` | Backing field for `Pieces` property |
| `tableSize` | `int` | Grid dimension |
| `piecesMaxValue` | `int` | Maximum random value (constant = 102) |

### Constructor

```csharp
public GameTable(int tableSize)
{
    this.tableSize = tableSize;
    InitializePieces();
    GenerateTable();
}
```

1. Stores `tableSize`
2. Calls `InitializePieces()` — allocates `Piece[,]` matrix
3. Calls `GenerateTable()` — places random matching pairs

### InitializePieces

```csharp
private void InitializePieces()
{
    pieces = new Piece[tableSize, tableSize];
    for (int i = 0; i < tableSize; i++)
        for (int j = 0; j < tableSize; j++)
        {
            pieces[i, j] = new Piece();
            pieces[i, j].PositionX = i;
            pieces[i, j].PositionY = j;
        }
}
```

- Allocates `Piece[tableSize, tableSize]`
- Creates `Piece` instance for every cell
- Sets `PositionX = i`, `PositionY = j` (column, row)
- All pieces start with `Value = -1` (uninitialised), `IsFlipped = false`, `IsCleared = false`

### GenerateTable

```csharp
private void GenerateTable()
{
    Random rnd = new Random();
    List<int> valuesUsed = new List<int>();
    int piecesCount = tableSize * tableSize / 2;
    int val = -1;

    for (int i = 0; i < piecesCount; i++)
    {
        int placed = 0;

        // Pick unique value not used yet
        while (val == -1 || valuesUsed.Contains(val))
            val = rnd.Next(0, piecesMaxValue);  // 0–101
        valuesUsed.Add(val);

        // Place value in two random empty cells
        while (placed < 2)
        {
            int x = rnd.Next(0, TableSize);
            int y = rnd.Next(0, TableSize);

            if (pieces[x, y].Value < 0)  // Empty cell
            {
                pieces[x, y].Value = val;
                placed += 1;
            }
        }
    }
}
```

**Algorithm:**
1. Calculate `piecesCount = tableSize² / 2` (number of pairs)
2. For each pair (0 to piecesCount-1):
   - Generate random `val` in [0, 101] not already used
   - Add to `valuesUsed` list
   - Place `val` in two random cells where `Value < 0`
3. Result: exactly two of each value, all cells filled

**Properties:**
- `piecesMaxValue = 102` (constant, allows values 0–101)
- Maximum pairs for 8×8 grid: 64/2 = 32 pairs (well within 102)
- `Random` instance created per generation (not shared)
- `valuesUsed` ensures uniqueness across pairs

### Regenerate

```csharp
public void Regenerate()
{
    ClearTable();
    InitializePieces();
    GenerateTable();
}
```

- Clears existing pieces (sets to null, then nulls array)
- Re-allocates and regenerates
- Used for "retry same difficulty" — but `GameEngine.NewGame()` creates new `GameTable` instead

### ClearTable

```csharp
private void ClearTable()
{
    for (int i = 0; i < tableSize; i++)
        for (int j = 0; j < tableSize; j++)
            pieces[i, j] = null;
    pieces = null;
}
```

- Nulls each element, then nulls array reference
- Allows GC to collect `Piece` objects

### Indexing Convention

`pieces[x, y]` where:
- `x` = column (0 to tableSize-1), horizontal axis
- `y` = row (0 to tableSize-1), vertical axis
- Origin (0,0) = top-left
- Matches `Piece.PositionX` / `PositionY`
- Matches `MainWindow` coordinate calculation: `x = Event.X / tileSize`, `y = Event.Y / tileSize`

## Piece

**File:** `MemoryBlocks/Game/Piece.cs`
**Namespace:** `MemoryBlocks.Game`
**Class:** `Piece`

### Purpose

Data model representing an individual board tile. Pure data — no behaviour, no dependencies beyond `System`.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `int` | `-1` | Picture identifier (0–36 for assets, up to 101 generated) |
| `Size` | `int` | `48` | Legacy tile size (unused in rendering) |
| `PositionX` | `int` | `0` | Column index |
| `PositionY` | `int` | `0` | Row index |
| `IsFlipped` | `bool` | `false` | Face-up state |
| `IsCleared` | `bool` | `false` | Matched/removed state |

### Constructor

```csharp
public Piece()
{
    size = 48;
    posX = 0;
    posY = 0;
    isFlipped = false;
    isCleared = false;
    value = -1;
}
```

- `value = -1` marks uninitialised (used by `GenerateTable` to find empty cells)
- `size = 48` is legacy; actual tile size computed dynamically in `GameEngine.DrawTable()`

### State Transitions

```
[Value=-1, Flipped=false, Cleared=false]  --GenerateTable-->  [Value=0..101, Flipped=false, Cleared=false]
                                                                       |
                                                                       | FlipPiece (click)
                                                                       v
                                                              [Value=0..101, Flipped=true, Cleared=false]
                                                                       |
                                                    +----------------+----------------+
                                                    |                                 |
                                          CheckFlipped (match)              CheckFlipped (no match)
                                                    |                                 |
                                                    v                                 |
                                          [Value=0..101, Flipped=false, Cleared=true]  (stays flipped)
                                                    |                                 |
                                                    |                    Next FlipPiece (any tile)
                                                    |                                 |
                                                    +----------------+----------------+
                                                                     |
                                                                     v
                                                          [Value=0..101, Flipped=false, Cleared=false]
                                                          (unflipped by UnflipAllPieces)
```

### Invariants

1. `Value == -1` only during `GenerateTable` (transient)
2. `IsCleared == true` → `IsFlipped == false` (enforced by `CheckFlipped`)
3. `PositionX`, `PositionY` immutable after `InitializePieces`
4. `Size` constant (48), not used

### Equality

- No `IEquatable<Piece>` implementation
- No `Equals(object)` override
- No `GetHashCode()` override
- Reference equality only (sufficient — each cell has unique `Piece` instance)

## Dependencies

### GameTable Dependencies

| Dependency | Purpose |
|------------|---------|
| `System.Random` | Random value generation |
| `System.Collections.Generic.List<int>` | Track used values |
| `Piece` | Element type |

### Piece Dependencies

| Dependency | Purpose |
|------------|---------|
| `System` | Basic types only |

## Coupling

- **GameTable → Piece:** Composition (owns `Piece[,]`)
- **GameEngine → GameTable:** Composition (owns, creates per session)
- **GameEngine → Piece:** Indirect (via `GameTable.Pieces[x, y]`)
- **MainWindow → GameTable/Piece:** None (only via `GameEngine`)

## Testing Considerations

- `GameTable` and `Piece` have zero external dependencies — easily unit testable
- `GenerateTable` algorithm deterministic given `Random` seed (could inject `Random` for testing)
- No GTK#, GDK, or System.Drawing references in domain models
- Current codebase has no tests

## Asset Coupling

**Critical Constraint:** `GameTable` generates values 0–101, but only 37 piece images exist (`piece0.png`–`piece36.png`).

```csharp
// GameEngine.DrawTable()
Bitmap bmp = Gdk.Pixbuf.LoadFromResource(
    "MemoryBlocks.Resources.Pieces.piece" + piece.Value + ".png").ToBitmap();
```

If `piece.Value >= 37`, resource load fails at runtime.

**Current Mitigation:** 8×8 grid = 32 pairs max; `piecesMaxValue = 102` provides large space but collision probability low. However, values 37–101 will crash.

**Fix Options:**
1. Reduce `piecesMaxValue` to 37
2. Add more piece assets (37–101)
3. Modulo value: `piece.Value % 37` when loading resource
4. Validate in `GenerateTable` and retry if value ≥ 37