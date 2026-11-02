# Tile Flip and Match Flow

This document describes the tile flip, match evaluation, and board update sequence.

## Sequence Diagram

```
User clicks daTable
    │
    ├─► OnDaTableButtonPressEvent(args)
    │       ├─► if (!game.IsRunning) return
    │       ├─► tileSize = daTable.GdkWindow.GetWidth() / game.TableSize
    │       ├─► x = (int)args.Event.X / tileSize
    │       ├─► y = (int)args.Event.Y / tileSize
    │       ├─► if (args.Event.Button == 1)  // Left click
    │       │       └─► game.FlipPiece(x, y)
    │       ├─► game.DrawTable()
    │       ├─► game.DrawInfoBar()
    │       └─► if (game.Completed) ShowDialog("Level complete", ...)
    │
    └─► (returns to GTK main loop)
```

## FlipPiece Detailed Flow

```
FlipPiece(x, y)
    │
    ├─► piece = gameTable.Pieces[x, y]
    │
    ├─► if (piece.IsCleared) return  // Ignore cleared tiles
    │
    ├─► if (piece.IsFlipped)
    │       │   // Unflip: user clicked already-flipped tile
    │       ├─► piece.IsFlipped = false
    │       ├─► flips -= 1
    │       └─► (falls through to DrawTable/DrawInfoBar)
    │
    └─► else  // Flip: user clicked face-down tile
            │
            ├─► if (flips == 2)
            │       └─► UnflipAllPieces()  // Enforce max 2 flipped
            │
            ├─► piece.IsFlipped = true
            ├─► flips += 1
            │
            ├─► if (flips == 2)
            │       └─► CheckFlipped()
            │
            └─► (falls through to DrawTable/DrawInfoBar)
```

## CheckFlipped Detailed Flow

```
CheckFlipped()
    │
    ├─► flip1i = -1, flip1j = -1
    │
    ├─► for i in 0..tableSize-1
    │       for j in 0..tableSize-1
    │           if (gameTable.Pieces[i, j].IsFlipped)
    │               │
    │               ├─► if (flip1i == -1)  // First flipped tile found
    │               │       ├─► flip1i = i, flip1j = j
    │               │       └─► continue
    │               │
    │               └─► else  // Second flipped tile found
    │                       │
    │                       ├─► val1 = gameTable.Pieces[flip1i, flip1j].Value
    │                       ├─► val2 = gameTable.Pieces[i, j].Value
    │                       │
    │                       ├─► if (val1 == val2)  // MATCH!
    │                       │       ├─► gameTable.Pieces[flip1i, flip1j].IsCleared = true
    │                       │       ├─► gameTable.Pieces[flip1i, flip1j].IsFlipped = false
    │                       │       ├─► gameTable.Pieces[i, j].IsCleared = true
    │                       │       ├─► gameTable.Pieces[i, j].IsFlipped = false
    │                       │       ├─► flips = 0
    │                       │       ├─► piecesLeft -= 1
    │                       │       └─► return
    │                       │
    │                       └─► else  // NO MATCH
    │                               ├─► (leave both IsFlipped = true)
    │                               ├─► flips remains 2
    │                               └─► return
    │
    └─► (implicit return if < 2 flipped — should not happen)
```

## UnflipAllPieces Detailed Flow

```
UnflipAllPieces()
    │
    ├─► for i in 0..tableSize-1
    │       for j in 0..tableSize-1
    │           gameTable.Pieces[i, j].IsFlipped = false
    │
    └─► flips = 0
```

## State Transitions Per Click

### Click 1 (First Tile)

```
Before: flips=0, piecesLeft=N, all tiles unflipped
Action: FlipPiece(x1, y1)
  piece.IsFlipped = true
  flips = 1
After:  flips=1, one tile flipped
Render: DrawTable shows 1 face-up tile with image
```

### Click 2 (Second Tile — Match)

```
Before: flips=1, tile A flipped
Action: FlipPiece(x2, y2) where piece.Value == tileA.Value
  piece.IsFlipped = true
  flips = 2
  CheckFlipped() → MATCH
    Both tiles: IsCleared=true, IsFlipped=false
    flips = 0
    piecesLeft -= 1
After:  flips=0, piecesLeft=N-1, two tiles cleared (invisible)
Render: DrawTable shows cleared tiles as background (empty)
```

### Click 2 (Second Tile — No Match)

```
Before: flips=1, tile A flipped
Action: FlipPiece(x2, y2) where piece.Value != tileA.Value
  piece.IsFlipped = true
  flips = 2
  CheckFlipped() → NO MATCH
    Both tiles remain IsFlipped=true
    flips = 2
After:  flips=2, two tiles face-up (different images)
Render: DrawTable shows both face-up tiles
```

### Click 3 (After No Match — Unflip)

```
Before: flips=2, tiles A and B face-up (different)
Action: FlipPiece(x3, y3) — any tile (including A or B)
  flips == 2 → UnflipAllPieces()
    All tiles IsFlipped = false
    flips = 0
  Then: piece.IsFlipped = true (for clicked tile)
  flips = 1
After:  flips=1, only clicked tile face-up
Render: DrawTable shows only new tile face-up
```

### Click on Cleared Tile

```
Before: tile C is cleared (IsCleared=true)
Action: FlipPiece(xC, yC)
  if (piece.IsCleared) return  // No-op
After:  No state change
Render: No visual change
```

### Click on Already-Flipped Tile (Unflip)

```
Before: flips=1, tile A flipped
Action: FlipPiece(xA, yA) — same tile
  piece.IsFlipped = false
  flips = 0
After:  flips=0, no tiles flipped
Render: DrawTable shows all face-down
```

## Completion Detection

```
After each FlipPiece → DrawTable → DrawInfoBar:
    │
    └─► if (game.Completed)  // piecesLeft == 0
            └─► ShowDialog("Level complete", "You have successfully cleared this level!", Info, Ok)
```

**Completed Property:**
```csharp
public bool Completed { get { return piecesLeft == 0; } }
```

- Checked in `MainWindow.OnDaTableButtonPressEvent` after render
- `piecesLeft` decremented only in `CheckFlipped()` on match
- Initial `piecesLeft = (tableSize * tableSize) / 2`
- When `piecesLeft` reaches 0, all pairs matched

## Timer Interaction

```
TimerTick() (every ~1000ms)
    │
    ├─► if (!isRunning) return
    │
    ├─► if (piecesLeft == 0)
    │       ├─► isRunning = false
    │       └─► return true
    │
    ├─► gameTime += 1
    ├─► DrawInfoBar()  // Updates timer display
    └─► return true
```

- Timer continues during gameplay
- Stops automatically when `piecesLeft == 0` (game complete)
- `isRunning = false` prevents further timer ticks
- `DrawInfoBar` called from both timer and click handler

## Rendering After Flip

### DrawTable (Post-Flip)

**Flipped Tile Rendering:**
```csharp
if (piece.IsFlipped)
{
    gfx.FillRectangle(brFlipped, rec);  // LightBlue background
    Bitmap bmp = Gdk.Pixbuf.LoadFromResource(
        "MemoryBlocks.Resources.Pieces.piece" + piece.Value + ".png").ToBitmap();
    gfx.DrawImage(bmp, rec);  // Draw piece image
}
```

**Unflipped, Not Cleared:**
```csharp
else if (!piece.IsCleared)
    gfx.FillRectangle(brUnflipped, rec);  // CornflowerBlue
```

**Cleared:**
```csharp
// No drawing — background shows through
```

### DrawInfoBar (Post-Flip)

Updates three sections:
1. **Left:** `piecesLeft.ToString()` — decrements on match
2. **Middle:** `":)"` — static
3. **Right:** `string.Format("{0:00}:{1:00}", (gameTime/60)%60, gameTime%60)` — updates every second via timer

## Coordinate System

```
daTable (DrawingArea)
    │
    ├─► Origin (0,0) = top-left
    ├─► X increases rightward
    ├─► Y increases downward
    │
    ├─► tileSize = daTable.GdkWindow.GetWidth() / tableSize
    │       (assumes square drawing area)
    │
    ├─► x = (int)Event.X / tileSize  // Column
    └─► y = (int)Event.Y / tileSize  // Row
```

**Matches GameTable Indexing:**
- `gameTable.Pieces[x, y]` where x=column, y=row
- `Piece.PositionX = x`, `Piece.PositionY = y` (set in `InitializePieces`)

## Edge Cases

| Scenario | Handling |
|----------|----------|
| Click outside grid | `x, y` may exceed bounds → `IndexOutOfRangeException` (not guarded) |
| Right click (button 3) | No-op (empty `else if` block) |
| Click during modal dialog | `isRunning` still true, but main loop blocked — click queued |
| Click after completion | `game.Completed` true → dialog shown, but `isRunning` false → `FlipPiece` early returns |
| Rapid clicks | GTK serialises events — processed sequentially |
| `piece.Value >= 37` | `LoadFromResource` fails → exception at render time |

## Invariants Maintained

1. `flips ∈ {0, 1, 2}` — never exceeds 2
2. At most 2 tiles have `IsFlipped == true`
3. `IsCleared == true` → `IsFlipped == false`
4. `piecesLeft` = count of pairs not yet cleared
5. `isRunning == true` while `piecesLeft > 0`
6. `Completed == (piecesLeft == 0)`
7. `gameTime` increments only while `isRunning`

## Performance Characteristics

| Operation | Complexity | Notes |
|-----------|------------|-------|
| `FlipPiece` | O(1) | Direct array access |
| `CheckFlipped` | O(tableSize²) | Scans entire board (max 64 cells) |
| `UnflipAllPieces` | O(tableSize²) | Scans entire board |
| `DrawTable` | O(tableSize²) | Renders all cells |
| `DrawInfoBar` | O(1) | Fixed three sections |

- All operations fast for tableSize ≤ 8
- No optimisation needed for current scale
- `CheckFlipped` scans full board instead of tracking flipped indices (simple, acceptable)

## Resource Loading on Flip

**Lazy Loading:** Piece images loaded only when tile flipped (`piece.IsFlipped == true`)

```csharp
Bitmap bmp = Gdk.Pixbuf.LoadFromResource(
    "MemoryBlocks.Resources.Pieces.piece" + piece.Value + ".png").ToBitmap();
```

- `piece.Value` used directly in resource name
- `ToBitmap()` extension converts `Gdk.Pixbuf` → `System.Drawing.Bitmap`
- `Bitmap` disposed via `using` in `DrawTable`
- **Risk:** `piece.Value >= 37` → resource not found → exception

## Error Propagation

| Error | Origin | Propagation |
|-------|--------|-------------|
| `IndexOutOfRangeException` | `gameTable.Pieces[x, y]` | Unhandled → crashes GTK main loop |
| `NullReferenceException` | `gdkWindowTable`/`gdkWindowInfoBar` null | Unhandled → crashes render |
| `Exception` | `LoadFromResource` (missing asset) | Unhandled → crashes render |
| `OutOfMemoryException` | `Bitmap` allocation | Unhandled → crashes process |

No try/catch in game logic — exceptions terminate application.