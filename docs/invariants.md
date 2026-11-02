# Invariants

This document lists the system invariants that must hold at all times during execution.

## Game State Invariants

### I1: Flip Count Bound
**Invariant:** `0 ≤ flips ≤ 2`
**Enforced by:** `FlipPiece`, `UnflipAllPieces`, `CheckFlipped`
**Violated if:** Direct field manipulation, concurrent access (not possible currently)

### I2: Pieces Left Non-Negative
**Invariant:** `piecesLeft ≥ 0`
**Enforced by:** `CheckFlipped` (only decrements on match), `NewGame` (initialises to positive)
**Violated if:** `CheckFlipped` called incorrectly, integer underflow

### I3: Completed Equivalence
**Invariant:** `Completed ⇔ piecesLeft == 0`
**Enforced by:** `Completed` property getter
**Violated if:** `piecesLeft` modified without going through `CheckFlipped`

### I4: Running State Consistency
**Invariant:** `isRunning ⇔ piecesLeft > 0`
**Enforced by:** `NewGame` (sets both), `TimerTick` (stops when `piecesLeft == 0`), `CheckFlipped` (decrements `piecesLeft`)
**Violated if:** `isRunning` set directly, `TimerTick` exception

### I5: Flip Count Matches Flipped Tiles
**Invariant:** `flips = count(p where p.IsFlipped)`
**Enforced by:** `FlipPiece` (increments/decrements with `IsFlipped`), `UnflipAllPieces` (resets both), `CheckFlipped` (resets both on match)
**Violated if:** `Piece.IsFlipped` modified directly

### I6: Cleared Tiles Never Flipped
**Invariant:** `∀p: p.IsCleared → ¬p.IsFlipped`
**Enforced by:** `CheckFlipped` (sets `IsCleared=true` and `IsFlipped=false` atomically)
**Violated if:** `Piece.IsFlipped` set to true on cleared tile

### I7: Pieces Left Matches Uncleared Pairs
**Invariant:** `piecesLeft = (totalTiles - count(p where p.IsCleared)) / 2`
**Enforced by:** `NewGame` (initialises), `CheckFlipped` (decrements on match, clears 2 tiles)
**Violated if:** `piecesLeft` modified incorrectly, tiles cleared without match

### I8: Timer Runs While Game Active
**Invariant:** `isRunning → TimerTick registered`
**Enforced by:** `GameEngine` constructor (registers once), `NewGame` (does not re-register), `TimerTick` returns `isRunning`
**Violated if:** `GLib.Timeout` removed externally, `TimerTick` returns false incorrectly

## Board Structure Invariants

### I9: Board Dimensions
**Invariant:** `gameTable.Pieces.GetLength(0) == gameTable.Pieces.GetLength(1) == tableSize`
**Enforced by:** `GameTable` constructor
**Violated if:** Array replaced with different size

### I10: Valid Positions
**Invariant:** `∀p ∈ Pieces: 0 ≤ p.PositionX < tableSize ∧ 0 ≤ p.PositionY < tableSize`
**Enforced by:** `GameTable.InitializePieces`
**Violated if:** `PositionX/Y` modified after initialisation

### I11: Unique Positions
**Invariant:** `∀p1,p2 ∈ Pieces: p1 ≠ p2 → (p1.PositionX, p1.PositionY) ≠ (p2.PositionX, p2.PositionY)`
**Enforced by:** `InitializePieces` (nested loops with unique indices)
**Violated if:** Pieces array corrupted

### I12: Piece Values in Range
**Invariant:** `∀p ∈ Pieces: 0 ≤ p.Value < piecesMaxValue`
**Enforced by:** `GameTable.GenerateTable` (uses `i < pairs ≤ piecesMaxValue/2`)
**Current Status:** **HOLDS** (max value 31 for 8×8, `piecesMaxValue=102`)
**Risk:** If `tableSize` increased or algorithm changed, could exceed asset limit (37)

### I13: Each Value Appears Exactly Twice
**Invariant:** `∀v: count(p where p.Value == v) ∈ {0, 2}`
**Enforced by:** `GenerateTable` (creates pairs, shuffles)
**Violated if:** Shuffle algorithm bug, array corruption

## Rendering Invariants

### I14: Drawables Set Before Draw
**Invariant:** `gdkWindowTable ≠ null ∧ gdkWindowInfoBar ≠ null` when `DrawTable`/`DrawInfoBar` called
**Enforced by:** `NewGame` (sets from `MainWindow` realised widgets), `MainWindow` ensures realisation
**Violated if:** `DrawTable` called before `NewGame`, widgets not realised

### I15: Tile Size Positive
**Invariant:** `tileSize > 0` in `OnDaTableButtonPressEvent`
**Enforced by:** `daTable.GdkWindow.GetWidth() > 0` (widget realised), `tableSize > 0`
**Violated if:** Drawing area not allocated, `tableSize == 0`

### I16: Click Coordinates In Bounds
**Invariant:** `0 ≤ x < tableSize ∧ 0 ≤ y < tableSize` for `FlipPiece(x, y)`
**Current Status:** **NOT ENFORCED** — **BUG**
**Risk:** Click outside drawing area → `IndexOutOfRangeException`

## Resource Invariants

### I17: Piece Images Exist for All Values
**Invariant:** `∀p ∈ Pieces: resource "piece{p.Value}.png" exists`
**Current Status:** **VIOLATED** — `piecesMaxValue=102` but only 37 images (0–36)
**Actual Safe Range:** `p.Value ≤ 36`
**Mitigation:** Clamp values in `GenerateTable` to 36, or increase assets

### I18: Brushes and Fonts Not Disposed Prematurely
**Invariant:** `brBackground`, `brUnflipped`, `brFlipped`, `brInfoBackground`, `brInfoText`, `fontInfo` valid for `GameEngine` lifetime
**Enforced by:** Created in `NewGame`, never disposed, GC handles
**Violated if:** Explicit `Dispose()` called

## UI Invariants

### I19: Main Window Visible While Running
**Invariant:** `MainWindow.Visible == true` during `Application.Run()`
**Enforced by:** GTK main loop, `MainWindow` shown before `Application.Run()`
**Violated if:** `Hide()` called, window destroyed

### I20: Menu Actions Available
**Invariant:** Easy/Moderate/Hard/Exit menu items sensitive while running
**Enforced by:** GTK default sensitivity, no code disables them
**Violated if:** Menu items explicitly desensitized

## Concurrency Invariants

### I21: Single-Threaded Access
**Invariant:** All `GameEngine`, `GameTable`, `Piece` access on GTK main thread
**Enforced by:** Architecture (no background threads), `GLib.Timeout` runs on main loop
**Violated if:** Background thread introduced

### I22: No Reentrancy in Game Logic
**Invariant:** `FlipPiece`, `CheckFlipped`, `UnflipAllPieces` not called reentrantly
**Enforced by:** GTK serialises events, timer callback not reentrant
**Violated if:** `FlipPiece` called from `TimerTick` or nested event

## Initialisation Invariants

### I23: NewGame Establishes Clean State
**Invariant:** After `NewGame(n)`: `tableSize=n`, `isRunning=true`, `gameTime=0`, `flips=0`, `piecesLeft=n²/2`, new `GameTable`, new `Piece[,]`
**Enforced by:** `NewGame` implementation
**Violated if:** `NewGame` modified incompletely

### I24: Timer Registered Once
**Invariant:** Exactly one `GLib.Timeout` source for `TimerTick` per process
**Enforced by:** `GameEngine` constructor called once per `NewGame`, but `GameEngine` recreated each `NewGame` — **VIOLATED** (old timer not removed)
**Actual Behaviour:** Each `NewGame` creates new `GameEngine` → new timer → multiple timers running
**Fix:** Store timer ID, remove in `NewGame` before creating new `GameEngine`

## Summary Table

| ID | Invariant | Status | Critical |
|----|-----------|--------|----------|
| I1 | `0 ≤ flips ≤ 2` | ✅ Held | Yes |
| I2 | `piecesLeft ≥ 0` | ✅ Held | Yes |
| I3 | `Completed ⇔ piecesLeft == 0` | ✅ Held | Yes |
| I4 | `isRunning ⇔ piecesLeft > 0` | ⚠️ Timer bug | Yes |
| I5 | `flips = count(IsFlipped)` | ✅ Held | Yes |
| I6 | `IsCleared → ¬IsFlipped` | ✅ Held | Yes |
| I7 | `piecesLeft = uncleared/2` | ✅ Held | Yes |
| I8 | Timer registered while running | ⚠️ Multiple timers | Yes |
| I9 | Board square, correct size | ✅ Held | Yes |
| I10 | Positions in bounds | ✅ Held | Yes |
| I11 | Unique positions | ✅ Held | Yes |
| I12 | Values < piecesMaxValue | ✅ Held (incidental) | Yes |
| I13 | Each value twice | ✅ Held | Yes |
| I14 | Drawables set before draw | ✅ Held | Yes |
| I15 | Tile size positive | ✅ Held | Yes |
| I16 | Click coords in bounds | ❌ **VIOLATED** | **Critical** |
| I17 | Piece images exist | ❌ **VIOLATED** | **Critical** |
| I18 | Brushes/fonts valid | ✅ Held | No |
| I19 | Window visible | ✅ Held | No |
| I20 | Menu available | ✅ Held | No |
| I21 | Single-threaded | ✅ Held | Yes |
| I22 | No reentrancy | ✅ Held | Yes |
| I23 | NewGame clean state | ✅ Held | Yes |
| I24 | Single timer | ❌ **VIOLATED** | Medium |

## Invariant Checking (Recommended)

### Debug Build Assertions
```csharp
// In GameEngine methods
[Conditional("DEBUG")]
private void AssertInvariants()
{
    Debug.Assert(flips >= 0 && flips <= 2, "I1");
    Debug.Assert(piecesLeft >= 0, "I2");
    Debug.Assert(Completed == (piecesLeft == 0), "I3");
    Debug.Assert(isRunning == (piecesLeft > 0), "I4");
    Debug.Assert(flips == gameTable.Pieces.Cast<Piece>().Count(p => p.IsFlipped), "I5");
    Debug.Assert(!gameTable.Pieces.Cast<Piece>().Any(p => p.IsCleared && p.IsFlipped), "I6");
}
```

### Runtime Validation (Production)
```csharp
private void ValidateInvariants()
{
    if (flips < 0 || flips > 2) throw new InvalidOperationException("I1");
    if (piecesLeft < 0) throw new InvalidOperationException("I2");
    // ...
}
```

Call at start/end of public methods in debug builds.