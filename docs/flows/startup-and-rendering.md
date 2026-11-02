# Startup and Rendering Flow

This document describes the application initialisation and first render sequence.

## Sequence Diagram

```
MainClass.Main()
    │
    ├─► Application.Init()
    │       └─► GTK# type system, thread init, event loop setup
    │
    ├─► new MainWindow()
    │       │
    │       ├─► base(Gtk.WindowType.Toplevel)
    │       │
    │       ├─► Build()  [Stetic-generated]
    │       │       ├─► Creates widget tree: Window → VBox → MenuBar, HBox, Statusbar
    │       │       ├─► HBox → daTable (DrawingArea), VBox → daInfoBar (DrawingArea)
    │       │       ├─► Wire signals: ButtonPressEvent, ExposeEvent, Activated
    │       │       └─► Assign widget fields: daTable, daInfoBar, actEasy, etc.
    │       │
    │       ├─► daTable.DoubleBuffered = false
    │       │
    │       ├─► game = new GameEngine(6)
    │       │       ├─► backgroundColor = GhostWhite
    │       │       ├─► primaryColor = CornflowerBlue
    │       │       ├─► secondaryColor = LightBlue
    │       │       └─► GLib.Timeout.Add(1000, TimerTick)  ──► Timer registered
    │       │
    │       ├─► game.GdkWindowTable = daTable.GdkWindow
    │       ├─► game.GdkWindowInfoBar = daInfoBar.GdkWindow
    │       │
    │       ├─► game.NewGame(6)
    │       │       ├─► gameTable = new GameTable(6)
    │       │       │       ├─► InitializePieces()
    │       │       │       │       └─► pieces = new Piece[6,6]; create 36 Piece objects
    │       │       │       └─► GenerateTable()
    │       │       │               ├─► piecesCount = 18
    │       │       │               ├─► For each pair: pick unique val ∈ [0,101], place in 2 random cells
    │       │       │               └─► All 36 cells filled with 18 matching pairs
    │       │       ├─► tableSize = 6, tileSpacing = 2
    │       │       ├─► gameTime = 0, flips = 0, piecesLeft = 18
    │       │       ├─► isRunning = true
    │       │       ├─► DrawTable()  ──► Initial grid render
    │       │       └─► DrawInfoBar() ──► Initial info bar render
    │       │
    │       ├─► daTable.ExposeEvent += delegate { game.DrawTable(); }
    │       ├─► daInfoBar.ExposeEvent += delegate { game.DrawInfoBar(); }
    │       └─► daTable.AddEvents(ButtonPressMask)
    │
    ├─► win.Show()
    │       └─► GTK maps window, triggers initial ExposeEvent on drawing areas
    │               ├─► daTable.ExposeEvent → game.DrawTable()
    │               └─► daInfoBar.ExposeEvent → game.DrawInfoBar()
    │
    └─► Application.Run()
            └─► GTK main loop (blocks)
                    ├─► Dispatches events: ButtonPress, Expose, Timeout, Delete
                    ├─► TimerTick fires every ~1000ms
                    └─► Runs until Application.Quit()
```

## Initial Render Details

### DrawTable (First Call)

**Trigger:** `game.NewGame(6)` → `DrawTable()` + `daTable.ExposeEvent` after `win.Show()`

**Steps:**
1. `if (!isRunning) return` — `isRunning == true`, continues
2. `gdkWindowTable.GetSize(out width, out height)` — gets drawing area dimensions
3. `tileSize = (width - 6 * 2) / 6` — computes tile size from width
4. `Graphics gfx = Gtk.DotNet.Graphics.FromDrawable(gdkWindowTable)` — GDI+ context
5. `gfx.FillRectangle(backgroundColor, fullRect)` — clears to GhostWhite
6. For each cell (x=0..5, y=0..5):
   - Compute `Rectangle rec` with spacing
   - `piece = gameTable.Pieces[x, y]` — all `IsFlipped=false`, `IsCleared=false`
   - `gfx.FillRectangle(primaryColor, rec)` — draws CornflowerBlue rectangle
7. `gfx.Dispose()` — cleanup

**Result:** 6×6 grid of blue rectangles (face-down tiles)

### DrawInfoBar (First Call)

**Trigger:** `game.NewGame(6)` → `DrawInfoBar()` + `daInfoBar.ExposeEvent` after `win.Show()`

**Steps:**
1. `if (!isRunning) return` — continues
2. `gdkWindowInfoBar.GetSize(out width, out height)`
3. `widthThird = width / 3`
4. `Graphics gfx = Gtk.DotNet.Graphics.FromDrawable(gdkWindowInfoBar)`
5. `gfx.FillRectangle(backgroundColor, fullRect)` — GhostWhite background
6. `Font font = new Font("Sans", Min(width,height)*0.5, Regular)`
7. `StringFormat fmt = Center/Center`
8. Left third: `DrawString("18", font, primaryColor, leftRect, fmt)` — pieces left
9. Middle third: `DrawString(":)", font, primaryColor, midRect, fmt)` — face
10. Right third: `DrawString("00:00", font, primaryColor, rightRect, fmt)` — timer
11. `gfx.Dispose()`

**Result:** Info bar showing "18", ":)", "00:00"

## Timer Initialisation

**Location:** `GameEngine` constructor

```csharp
GLib.Timeout.Add(1000, new GLib.TimeoutHandler(TimerTick));
```

- Registers `TimerTick` callback on GTK main loop
- Interval: 1000ms (approximate)
- Runs on main loop thread (safe for `DrawInfoBar()`)
- Returns `true` to continue; `false` would cancel

**First Tick:** ~1 second after `Application.Run()` starts
- `isRunning == true`, `piecesLeft == 18`
- `gameTime` becomes 1
- `DrawInfoBar()` updates timer to "00:01"

## GTK Event Loop Integration

### Event Sources

| Source | Callback | Thread |
|--------|----------|--------|
| `daTable.ButtonPressEvent` | `OnDaTableButtonPressEvent` | Main loop |
| `daTable.ExposeEvent` | `delegate { game.DrawTable(); }` | Main loop |
| `daInfoBar.ExposeEvent` | `delegate { game.DrawInfoBar(); }` | Main loop |
| `GLib.Timeout` (1000ms) | `TimerTick` | Main loop |
| `MainWindow.DeleteEvent` | `OnDeleteEvent` | Main loop |
| Menu `Activated` signals | `OnAct*Activated` | Main loop |

### All Callbacks Execute On Main Loop Thread

- No cross-thread marshaling needed
- `GameEngine` methods called directly from event handlers
- `System.Drawing` operations on GDK surfaces safe on main thread

## Startup Invariants

1. `Application.Init()` called before any GTK widget creation
2. `MainWindow` constructed before `Application.Run()`
3. `GameEngine` created with valid `tableSize` (4, 6, or 8)
4. `GdkWindowTable` and `GdkWindowInfoBar` set before `NewGame()`
5. `NewGame()` called before `win.Show()` (initial render in constructor)
6. `ExposeEvent` handlers attached before `win.Show()`
6. `ButtonPressMask` added before `win.Show()`
7. Timer registered in `GameEngine` constructor (before `NewGame()`)

## Failure Modes

| Failure Point | Symptom | Cause |
|---------------|---------|-------|
| `Application.Init()` | Exception | GTK# not installed, display server unavailable |
| `Build()` | NullReferenceException | Stetic-generated code mismatch |
| `new GameEngine(6)` | TypeInitializationException | `System.Drawing` / `libgdiplus` missing |
| `GdkWindowTable` null | NullReferenceException in DrawTable | `daTable.GdkWindow` not realised yet |
| `LoadFromResource` | Exception | Resource name mismatch, asset missing |
| `Application.Run()` | Immediate exit | `Application.Quit()` called prematurely |

## Timing Diagram

```
Time →
────────────────────────────────────────────────────────────────────►

MainClass.Main()
    │
    ├─ Application.Init() ──────────────────────────────────────────►
    │
    ├─ new MainWindow() ────────────────────────────────────────────►
    │       │
    │       ├─ Build() ────────────────────────────────────────────►
    │       │
    │       ├─ new GameEngine(6) ──────────────────────────────────►
    │       │       └─ GLib.Timeout.Add(1000, TimerTick) ──────────► (armed)
    │       │
    │       ├─ game.GdkWindowTable = daTable.GdkWindow ────────────►
    │       ├─ game.GdkWindowInfoBar = daInfoBar.GdkWindow ────────►
    │       │
    │       ├─ game.NewGame(6) ────────────────────────────────────►
    │       │       ├─ new GameTable(6) ──────────────────────────►
    │       │       │       ├─ InitializePieces() ────────────────►
    │       │       │       └─ GenerateTable() ───────────────────►
    │       │       │
    │       │       ├─ DrawTable() ───────────────────────────────► (1st render)
    │       │       └─ DrawInfoBar() ─────────────────────────────► (1st render)
    │       │
    │       ├─ Event subscriptions ────────────────────────────────►
    │       │
    │       └─ (constructor returns) ──────────────────────────────►
    │
    ├─ win.Show() ─────────────────────────────────────────────────►
    │       └─ Triggers ExposeEvent on drawing areas ──────────────►
    │               ├─ DrawTable() ────────────────────────────────► (2nd render)
    │               └─ DrawInfoBar() ──────────────────────────────► (2nd render)
    │
    └─ Application.Run() ──────────────────────────────────────────► (blocks)
            │
            ├─ TimerTick @ ~1s ────────────────────────────────────►
            │       └─ DrawInfoBar() ──────────────────────────────► (timer update)
            │
            ├─ TimerTick @ ~2s ────────────────────────────────────►
            │       └─ DrawInfoBar() ──────────────────────────────►
            │
            ├─ User Click ─────────────────────────────────────────►
            │       └─ OnDaTableButtonPressEvent ──────────────────►
            │               └─ FlipPiece → DrawTable → DrawInfoBar ──►
            │
            └─ ... (continues until Quit) ─────────────────────────►
```

## Resource Loading at Startup

**Embedded Resources Accessed:**
- `MemoryBlocks.Resources.Pieces.piece0.png` through `piece36.png` (lazy-loaded on first flip)
- `gui.stetic` (loaded by Stetic at `Build()` time)
- `memoryblocks.ico` (window icon, loaded by GTK)

**No Resources Loaded During Initial Render:**
- Initial `DrawTable()` only draws coloured rectangles (no piece images)
- Piece images loaded on-demand when `piece.IsFlipped == true`
- This avoids loading all 37 images at startup