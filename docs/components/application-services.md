# Application Services — GameEngine

This document describes `GameEngine`: session state, timer, game rules, and rendering.

## GameEngine

**File:** `MemoryBlocks/Game/GameEngine.cs`
**Namespace:** `MemoryBlocks.Game`
**Class:** `GameEngine` (sealed by convention — no inheritance)

### Public API

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `GdkWindowTable` | `Gdk.Window` | Drawing surface for game grid (setter-injected) |
| `GdkWindowInfoBar` | `Gdk.Window` | Drawing surface for info bar (setter-injected) |
| `Completed` | `bool` | `true` when `piecesLeft == 0` (read-only) |
| `GameTime` | `int` | Elapsed seconds in current game (read-only) |
| `IsRunning` | `bool` | `true` while game session active (read-only) |
| `TableSize` | `int` | Current grid dimension (read-only) |
| `TileSpacing` | `int` | Pixel gap between tiles (read/write) |
| `PrimaryColor` | `Color` | Unflipped tile colour (read/write) |
| `SecondaryColor` | `Color` | Flipped tile background colour (read/write) |
| `BackgroundColor` | `Color` | Board background colour (read/write) |

#### Methods

| Method | Description |
|--------|-------------|
| `GameEngine(int tableSize)` | Constructor — initialises colours, registers timer |
| `NewGame(int tableSize)` | Starts new session: creates `GameTable`, resets counters, renders |
| `DrawTable()` | Renders game grid to `GdkWindowTable` |
| `DrawInfoBar()` | Renders status bar to `GdkWindowInfoBar` |
| `FlipPiece(int x, int y)` | Handles tile flip logic, match checking, unflip |

### Internal State

| Field | Type | Initial Value | Modified By |
|-------|------|---------------|-------------|
| `gameTable` | `GameTable` | `null` | `NewGame()` |
| `tableSize` | `int` | constructor arg | `NewGame()` |
| `tileSpacing` | `int` | `2` | `NewGame()`, property |
| `gameTime` | `int` | `0` | `TimerTick()`, `NewGame()` |
| `flips` | `int` | `0` | `FlipPiece()`, `UnflipAllPieces()`, `CheckFlipped()` |
| `piecesLeft` | `int` | `(size*size)/2` | `NewGame()`, `CheckFlipped()` |
| `isRunning` | `bool` | `false` | `NewGame()`, `TimerTick()` |
| `gdkWindowTable` | `Gdk.Window` | `null` | Property setter |
| `gdkWindowInfoBar` | `Gdk.Window` | `null` | Property setter |
| `primaryColor` | `Color` | `CornflowerBlue` | Constructor, property |
| `secondaryColor` | `Color` | `LightBlue` | Constructor, property |
| `backgroundColor` | `Color` | `GhostWhite` | Constructor, property |

### Constructor

```csharp
public GameEngine(int tableSize)
{
    backgroundColor = Color.GhostWhite;
    primaryColor = Color.CornflowerBlue;
    secondaryColor = Color.LightBlue;

    GLib.Timeout.Add(1000, new GLib.TimeoutHandler(TimerTick));
}
```

- Sets default colours (hardcoded)
- Registers `TimerTick` on GTK main loop (1000ms interval)
- Does NOT create `GameTable` — deferred to `NewGame()`

### NewGame

```csharp
public void NewGame(int tableSize)
{
    gameTable = new GameTable(tableSize);
    this.tableSize = tableSize;
    tileSpacing = 2;
    gameTime = 0;
    flips = 0;
    piecesLeft = (tableSize * tableSize) / 2;
    isRunning = true;

    DrawTable();
    DrawInfoBar();
}
```

- Creates new `GameTable` (generates randomised board)
- Resets all session counters
- Sets `isRunning = true` (enables timer tick updates)
- Triggers initial render of both surfaces

### TimerTick

```csharp
private bool TimerTick()
{
    if (isRunning)
    {
        if (piecesLeft == 0)
        {
            isRunning = false;
            return true;
        }
        gameTime += 1;
        DrawInfoBar();
    }
    return true;  // Continue timeout
}
```

- Called every ~1000ms on GTK main loop thread
- Only updates if `isRunning == true`
- Auto-stops when `piecesLeft == 0` (sets `isRunning = false`)
- Increments `gameTime` and redraws info bar
- Returns `true` to keep timeout active

### FlipPiece

```csharp
public void FlipPiece(int x, int y)
{
    if (gameTable.Pieces[x, y].IsCleared)
        return;

    if (gameTable.Pieces[x, y].IsFlipped)
    { // Unflip it
        gameTable.Pieces[x, y].IsFlipped = false;
        flips -= 1;
    }
    else
    { // Flip it
        if (flips == 2)
            UnflipAllPieces();

        gameTable.Pieces[x, y].IsFlipped = true;
        flips += 1;
    }

    if (flips == 2)
        CheckFlipped();
}
```

**Preconditions:**
- `x, y` within `[0, tableSize)` (caller responsibility — `MainWindow` calculates)
- `gameTable` not null (ensured by `NewGame()`)

**Logic:**
1. Ignore clicks on cleared tiles
2. If tile already flipped → unflip it, decrement `flips`
3. If tile not flipped:
   - If `flips == 2` → unflip all first (enforces max 2 flipped)
   - Flip tile, increment `flips`
4. If `flips == 2` after this flip → check for match

### CheckFlipped

```csharp
private void CheckFlipped()
{
    int flip1i = -1, flip1j = -1;

    for (int i = 0; i < tableSize; i++)
        for (int j = 0; j < tableSize; j++)
            if (gameTable.Pieces[i, j].IsFlipped)
            {
                if (flip1i == -1 && flip1j == -1)
                {
                    flip1i = i; flip1j = j;
                }
                else
                {
                    if (gameTable.Pieces[flip1i, flip1j].Value == gameTable.Pieces[i, j].Value)
                    {
                        // Match!
                        gameTable.Pieces[flip1i, flip1j].IsCleared = true;
                        gameTable.Pieces[flip1i, flip1j].IsFlipped = false;
                        gameTable.Pieces[i, j].IsCleared = true;
                        gameTable.Pieces[i, j].IsFlipped = false;
                        flips = 0;
                        piecesLeft -= 1;
                    }
                    return;  // Only check first two flipped
                }
            }
}
```

- Scans board for flipped tiles (max 2 expected)
- Compares values of first two flipped tiles
- If match: mark both `IsCleared = true`, `IsFlipped = false`, reset `flips = 0`, decrement `piecesLeft`
- If no match: leaves both flipped (will be unflipped on next `FlipPiece` call)
- Returns after processing first pair (assumes max 2 flipped)

### UnflipAllPieces

```csharp
private void UnflipAllPieces()
{
    for (int i = 0; i < tableSize; i++)
        for (int j = 0; j < tableSize; j++)
            gameTable.Pieces[i, j].IsFlipped = false;
    flips = 0;
}
```

- Called when third flip attempted (`flips == 2` in `FlipPiece`)
- Unflips all tiles, resets `flips = 0`
- Ensures at most 2 tiles flipped simultaneously

### DrawTable

```csharp
public void DrawTable()
{
    if (!isRunning) return;

    int width, height;
    gdkWindowTable.GetSize(out width, out height);
    int tileSize = (width - tableSize * tileSpacing) / tableSize;

    using (Graphics gfx = Gtk.DotNet.Graphics.FromDrawable(gdkWindowTable))
    using (Brush brBackground = new SolidBrush(backgroundColor))
    using (Brush brUnflipped = new SolidBrush(primaryColor))
    using (Brush brFlipped = new SolidBrush(secondaryColor))
    {
        gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
        gfx.FillRectangle(brBackground, new Rectangle(0, 0, width, height));

        for (int y = 0; y < tableSize; y++)
            for (int x = 0; x < tableSize; x++)
            {
                Rectangle rec = new Rectangle(
                    tileSpacing / 2 + x * tileSize + x * tileSpacing,
                    tileSpacing / 2 + y * tileSize + y * tileSpacing,
                    tileSize, tileSize);

                Piece piece = gameTable.Pieces[x, y];

                if (piece.IsFlipped)
                {
                    gfx.FillRectangle(brFlipped, rec);
                    using (Bitmap bmp = Gdk.Pixbuf.LoadFromResource(
                        "MemoryBlocks.Resources.Pieces.piece" + piece.Value + ".png").ToBitmap())
                    {
                        gfx.DrawImage(bmp, rec);
                    }
                }
                else if (!piece.IsCleared)
                {
                    gfx.FillRectangle(brUnflipped, rec);
                }
                // Cleared tiles: draw nothing (background shows through)
            }
    }
}
```

**Rendering Details:**
- Early return if `!isRunning`
- Computes `tileSize` from drawing area width
- Uses `Gtk.DotNet.Graphics.FromDrawable()` for GDI+ on GDK surface
- `InterpolationMode.HighQualityBicubic` for image scaling
- Background fill first
- Iterates `[x, y]` (column-major: x outer, y inner — matches `Piece[,]` indexing)
- **Flipped:** Draw secondary background + piece image (loaded from embedded resource)
- **Unflipped, not cleared:** Draw primary color rectangle
- **Cleared:** Draw nothing (background visible)
- Explicit `Dispose()` via `using` for `Graphics`, `Brush`, `Bitmap`

**Resource Loading:**
- `Gdk.Pixbuf.LoadFromResource("MemoryBlocks.Resources.Pieces.piece{Value}.png")`
- `piece.Value` must be 0–36 (only 37 assets exist; `piecesMaxValue = 102` allows higher)
- `.ToBitmap()` extension converts `Pixbuf` → `System.Drawing.Bitmap`

### DrawInfoBar

```csharp
public void DrawInfoBar()
{
    if (!isRunning) return;

    int width, height;
    gdkWindowInfoBar.GetSize(out width, out height);
    int widthThird = width / 3;

    using (Graphics gfx = Gtk.DotNet.Graphics.FromDrawable(gdkWindowInfoBar))
    using (Brush brBackground = new SolidBrush(backgroundColor))
    using (Brush brForeground = new SolidBrush(primaryColor))
    {
        Font font = new Font("Sans", (int)(Math.Min(width, height) * 0.5), FontStyle.Regular);
        StringFormat fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        gfx.FillRectangle(brBackground, new Rectangle(0, 0, width, height));

        // Left: pieces left
        gfx.DrawString(piecesLeft.ToString(), font, brForeground, new Rectangle(0, 0, widthThird, height), fmt);
        // Middle: face
        gfx.DrawString(":)", font, brForeground, new Rectangle(widthThird, 0, widthThird, height), fmt);
        // Right: timer MM:SS
        gfx.DrawString(string.Format("{0:00}:{1:00}", (gameTime / 60) % 60, gameTime % 60), font, brForeground, new Rectangle(widthThird * 2, 0, widthThird, height), fmt);
    }
}
```

**Layout:** Three equal horizontal thirds
- Left: `piecesLeft` count
- Middle: static face `":)"`
- Right: formatted timer `MM:SS`

**Font:** Dynamic size based on drawing area dimensions (`Min(width, height) * 0.5`)

## Dependencies

| Dependency | Purpose |
|------------|---------|
| `GameTable` | Board data, piece access |
| `Gdk.Window` | Drawing surface references |
| `Gtk.DotNet.Graphics` | GDK → GDI+ bridge |
| `System.Drawing` | `Graphics`, `Brush`, `Font`, `Bitmap`, `Rectangle`, `Color` |
| `GLib.Timeout` | Timer registration |
| `Gdk.Pixbuf` | Embedded resource loading |

## Invariants

1. `piecesLeft` = number of uncleared pairs remaining
2. `flips` ∈ {0, 1, 2} — never exceeds 2
3. At most 2 tiles have `IsFlipped == true` at any time
4. `isRunning == true` ↔ game session active (timer ticks, input accepted)
5. `Completed == true` ↔ `piecesLeft == 0`
6. `gameTable` not null when `isRunning == true`
7. `GdkWindowTable` and `GdkWindowInfoBar` must be set before `DrawTable`/`DrawInfoBar` called

## Error Conditions

| Condition | Handling |
|-----------|----------|
| `GdkWindowTable`/`GdkWindowInfoBar` null | `NullReferenceException` at render time |
| `piece.Value` ≥ 37 | `Gdk.Pixbuf.LoadFromResource` fails (resource not found) |
| `x, y` out of bounds | `IndexOutOfRangeException` in `gameTable.Pieces[x, y]` |
| `gdkWindow` disposed | GDK drawing error (visual corruption) |
| `libgdiplus` missing | `TypeInitializationException` for `System.Drawing` |

## Thread Safety

- **Not thread-safe** — all methods assume GTK main loop thread
- `TimerTick` runs on main loop (safe)
- Event handlers run on main loop (safe)
- No locking, no concurrent access