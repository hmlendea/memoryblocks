# Change Guide

This document describes how to make common changes to the codebase safely.

## Adding a New Difficulty Level

### Steps
1. **Add menu item** in Stetic designer (`gui.stetic`) or `MainWindow.Build()`
2. **Add handler** in `MainWindow.cs`:
```csharp
protected virtual void OnActExpertActivated(object sender, EventArgs e)
{
    game.NewGame(10);  // 10×10 grid
}
```
3. **Verify** `GameTable` supports size (max pairs = 50, values 0–49, needs 50 piece images)
4. **Add piece images** for values 37–49 (currently only 0–36 exist)
5. **Update** `piecesMaxValue` if exceeding 102

### Risk Areas
- Asset count must support `size²/2` unique values
- `piecesMaxValue` must be ≥ max value
- UI layout may need adjustment for larger grid

## Changing Colours

### Location
`GameEngine.NewGame()` — brush initialisation:
```csharp
brBackground = new SolidBrush(Color.GhostWhite);
brUnflipped = new SolidBrush(Color.CornflowerBlue);
brFlipped = new SolidBrush(Color.LightBlue);
brInfoBackground = new SolidBrush(Color.GhostWhite);
brInfoText = new SolidBrush(Color.Black);
```

### Process
1. Modify `Color` values
2. Rebuild
3. Test all states: unflipped, flipped, cleared, info bar

### Accessibility Note
Hardcoded colours prevent high-contrast mode. Consider `SystemColors` for future.

## Changing Timer Interval

### Location
`GameEngine` constructor:
```csharp
GLib.Timeout.Add(1000, new GLib.TimeoutHandler(TimerTick));
```

### Process
1. Change `1000` to desired milliseconds
2. Note: `gameTime` increments per tick, not per second
3. Update `DrawInfoBar` formatting if sub-second display needed

### Risk
- Too low (< 100ms) → excessive CPU, timer drift
- Too high → coarse time tracking

## Adding a New Piece Image

### Steps
1. Create PNG (recommended 48×48, transparent background)
2. Name: `piece{N}.png` where N = next unused value
3. Place in `MemoryBlocks/Resources/Pieces/`
4. Add to `MemoryBlocks.csproj`:
```xml
<EmbeddedResource Include="Resources\Pieces\piece37.png" />
```
5. Verify `piecesMaxValue` > N
6. Rebuild

### Naming Convention
- Must match `piece{value}.png` exactly
- Case-sensitive on Linux
- Value used directly in `DrawTable`

## Modifying Board Generation Algorithm

### Location
`GameTable.GenerateTable()`

### Current Algorithm
- Creates pairs 0..(pairs-1)
- Fisher-Yates shuffle
- Row-major fill

### Safe Modifications
- Change shuffle algorithm (keep uniform distribution)
- Change fill order (column-major, spiral, etc.)
- Add seed parameter for determinism

### Unsafe Modifications
- Non-uniform value distribution → unfair game
- Values outside 0..piecesMaxValue → crash
- Duplicate/non-pair values → unsolvable

## Changing Tile Size

### Current
- `Piece.Size = 48` (unused)
- `tileSize` calculated at runtime: `daTable.Width / tableSize`
- Rendering uses calculated `tileSize`

### To Change Visual Size
1. Modify drawing area size in Stetic/designer
2. Or override `daTable.SizeRequest` in code
3. `tileSize` auto-adjusts

### To Use Piece.Size
1. Replace calculated `tileSize` with `piece.Size`
2. Ensure drawing area accommodates `tableSize * piece.Size`

## Adding Keyboard Support

### Required Changes
1. **Add key event handler** to `daTable`:
```csharp
daTable.KeyPressEvent += OnDaTableKeyPressEvent;
daTable.CanFocus = true;
```
2. **Implement navigation**:
```csharp
protected virtual void OnDaTableKeyPressEvent(object o, KeyPressEventArgs args)
{
    switch (args.Event.Key) {
        case Gdk.Key.Left:  MoveFocus(-1, 0); break;
        case Gdk.Key.Right: MoveFocus(1, 0); break;
        case Gdk.Key.Up:    MoveFocus(0, -1); break;
        case Gdk.Key.Down:  MoveFocus(0, 1); break;
        case Gdk.Key.Return:
        case Gdk.Key.Space: FlipFocusedTile(); break;
    }
}
```
3. **Track focused tile** (new field in `GameEngine` or `MainWindow`)
4. **Visual indicator** for focused tile in `DrawTable`

## Adding Sound Effects

### Steps
1. Add sound files as embedded resources (`.wav`, `.ogg`)
2. Load in `NewGame`:
```csharp
flipSound = new SoundPlayer(Assembly.GetExecutingAssembly()
    .GetManifestResourceStream("MemoryBlocks.Resources.flip.wav"));
```
3. Play in `FlipPiece`, `CheckFlipped`:
```csharp
flipSound.Play();
```
4. Add `System.Media` reference

### Platform Note
`SoundPlayer` is Windows-only. For cross-platform, use `GStreamer` or `OpenAL`.

## Adding Game Statistics

### Data to Track
- Games played, won, lost
- Best time per difficulty
- Total flips, matches, mismatches
- Current streak

### Implementation
1. **Create `Statistics` class** with properties
2. **Persist to JSON** in `XDG_DATA_HOME` / `%LOCALAPPDATA%`
3. **Update in `GameEngine`**:
   - `NewGame` → gamesPlayed++
   - `CheckFlipped` match → matches++
   - `CheckFlipped` mismatch → mismatches++
   - Completion → gamesWon++, update best time
4. **Add UI** — new dialog or menu item

## Changing Font

### Location
`GameEngine.NewGame()`:
```csharp
fontInfo = new Font("Sans", 14);
```

### Process
1. Change font family, size, style
2. Test info bar layout (may need wider areas)
3. Consider DPI scaling

## Adding Animation

### Current
- Instant state changes (flip, clear)
- No animation

### Approach
1. **Add animation state** to `Piece`:
```csharp
public float FlipProgress { get; set; }  // 0.0 to 1.0
public bool IsAnimating { get; set; }
```
2. **Use `GLib.Timeout`** for animation frames (e.g., 60fps = 16ms)
3. **Interpolate in `DrawTable`** based on `FlipProgress`
4. **Block input** during animation

### Complexity
- Significant refactor of `DrawTable` and `FlipPiece`
- Consider if worth it for this project

## Refactoring GameEngine Drawing

### Current Coupling
- `GameEngine` directly uses `Gdk.Window` and `Gtk.DotNet.Graphics`
- Hard to test, hard to change backend

### Recommended Refactor
1. **Extract interface**:
```csharp
public interface IDrawingSurface
{
    void DrawRectangle(Brush brush, Rectangle rect);
    void DrawImage(Image image, Rectangle rect);
    void DrawString(string text, Font font, Brush brush, PointF point);
    Size GetSize();
}
```
2. **Implement `GdkDrawingSurface`** wrapping current code
3. **Inject into `GameEngine`** via constructor
4. **Create `TestDrawingSurface`** for unit tests

## Migrating to GTK 3/4

### Scope
- Major rewrite — not a simple change
- All UI code, rendering, event handling affected

### Steps
1. Update `.csproj` references to GTK 3/4 packages
2. Replace `Gtk.*` with `Gtk.*` (namespace same, API different)
3. Replace Stetic UI with manual code or Glade XML
4. Replace `gtk-dotnet` + `System.Drawing` with Cairo/SkiaSharp
5. Fix all breaking API changes
6. Test extensively

### Timeline Estimate
- 2–4 weeks for experienced GTK developer
- High risk of subtle bugs

## Checklist for Any Change

- [ ] Build succeeds (`xbuild MemoryBlocks.sln /p:Configuration=Release`)
- [ ] Application launches (`mono MemoryBlocks/bin/Release/MemoryBlocks.exe`)
- [ ] Basic gameplay works (flip, match, complete)
- [ ] All difficulty levels work
- [ ] No new exceptions in console
- [ ] Timer behaves correctly
- [ ] Info bar updates correctly
- [ ] "Level complete" dialog appears
- [ ] New Game resets state properly