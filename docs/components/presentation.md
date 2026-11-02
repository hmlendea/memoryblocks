# Presentation Layer

This document describes the UI layer: `MainWindow`, GTK widgets, and event binding.

## MainWindow

**File:** `MemoryBlocks/MainWindow.cs`
**Namespace:** `MemoryBlocks`
**Base Class:** `Gtk.Window`

### Construction

```csharp
public MainWindow() : base(Gtk.WindowType.Toplevel)
{
    this.Build();                           // Stetic-generated UI
    daTable.DoubleBuffered = false;         // Direct GDK drawing

    game = new GameEngine(6);               // Default 6x6 grid
    game.GdkWindowTable = daTable.GdkWindow;
    game.GdkWindowInfoBar = daInfoBar.GdkWindow;
    game.NewGame(6);                        // Start first game

    daTable.ExposeEvent += delegate { game.DrawTable(); };
    daInfoBar.ExposeEvent += delegate { game.DrawInfoBar(); };
    daTable.AddEvents((int)Gdk.EventMask.ButtonPressMask);
}
```

### Fields

| Field | Type | Description |
|-------|------|-------------|
| `game` | `GameEngine` | Game session orchestrator (owned) |

### Event Handlers

#### `OnDaTableButtonPressEvent`

**Signature:** `protected void OnDaTableButtonPressEvent(object o, ButtonPressEventArgs args)`

**Trigger:** Left/right mouse click on `daTable` drawing area

**Logic:**
```csharp
if (game.IsRunning)
{
    int tileSize = daTable.GdkWindow.GetWidth() / game.TableSize;
    int x = (int)args.Event.X / tileSize;
    int y = (int)args.Event.Y / tileSize;

    if (args.Event.Button == 1)      // Left click
        game.FlipPiece(x, y);
    else if (args.Event.Button == 3) // Right click (no-op)
        { }

    game.DrawTable();
    game.DrawInfoBar();

    if (game.Completed)
        ShowDialog("Level complete", "You have successfully cleared this level!", MessageType.Info, ButtonsType.Ok);
}
```

**Coordinate Translation:**
- `tileSize` = drawing area width / table size (assumes square)
- `x, y` = pixel coordinates / tileSize (integer division)

#### `OnDeleteEvent`

**Signature:** `protected void OnDeleteEvent(object sender, DeleteEventArgs a)`

**Action:** `Application.Quit(); a.RetVal = true;`

#### Menu Action Handlers

| Handler | Action | Engine Call |
|---------|--------|-------------|
| `OnActEasyActivated` | Easy (4×4) | `game.NewGame(4)` |
| `OnActModerateActivated` | Moderate (6×6) | `game.NewGame(6)` |
| `OnActHardActivated` | Hard (8×8) | `game.NewGame(8)` |
| `OnActRetryActivated` | Retry current | `game.NewGame(game.TableSize)` |
| `OnDiAboutActivated` | About dialog | `ShowDialog(...)` |

### Dialog Helper

**Method:** `public void ShowDialog(string title, string message, MessageType msgType, ButtonsType btnType)`

Creates modal `MessageDialog`, runs it, destroys it.

## Stetic-Generated UI

**File:** `MemoryBlocks/gtk-gui/MemoryBlocks.MainWindow.cs` (partial)
**File:** `MemoryBlocks/gtk-gui/generated.cs`
**File:** `MemoryBlocks/gtk-gui/gui.stetic`

### Widget Hierarchy (from `Build()`)

```
MainWindow (Gtk.Window)
├── VBox (main container)
│   ├── MenuBar
│   │   ├── Game Menu
│   │   │   ├── Easy (actEasy)
│   │   │   ├── Moderate (actModerate)
│   │   │   ├── Hard (actHard)
│   │   │   ├── Separator
│   │   │   ├── Retry (actRetry)
│   │   │   ├── Separator
│   │   │   └── Quit (actQuit)
│   │   └── Help Menu
│   │       └── About (diAbout)
│   ├── HBox (content area)
│   │   ├── daTable (DrawingArea) — game grid
│   │   └── VBox (side panel)
│   │       ├── daInfoBar (DrawingArea) — status bar
│   │       └── (spacer)
│   └── Statusbar
```

### Key Widget References (from `generated.cs`)

| Field | Type | Purpose |
|-------|------|---------|
| `daTable` | `Gtk.DrawingArea` | Game grid rendering surface |
| `daInfoBar` | `Gtk.DrawingArea` | Info bar (pieces left, timer) |
| `actEasy` | `Gtk.Action` | Easy difficulty menu item |
| `actModerate` | `Gtk.Action` | Moderate difficulty menu item |
| `actHard` | `Gtk.Action` | Hard difficulty menu item |
| `actRetry` | `Gtk.Action` | Retry menu item |
| `actQuit` | `Gtk.Action` | Quit menu item |
| `diAbout` | `Gtk.Action` | About menu item |

### Signal Connections (from `Build()`)

| Widget | Signal | Handler |
|--------|--------|---------|
| `daTable` | `ButtonPressEvent` | `OnDaTableButtonPressEvent` |
| `daTable` | `ExposeEvent` | `delegate { game.DrawTable(); }` |
| `daInfoBar` | `ExposeEvent` | `delegate { game.DrawInfoBar(); }` |
| `actEasy` | `Activated` | `OnActEasyActivated` |
| `actModerate` | `Activated` | `OnActModerateActivated` |
| `actHard` | `Activated` | `OnActHardActivated` |
| `actRetry` | `Activated` | `OnActRetryActivated` |
| `actQuit` | `Activated` | `OnDeleteEvent` (via `Application.Quit`) |
| `diAbout` | `Activated` | `OnDiAboutActivated` |
| `MainWindow` | `DeleteEvent` | `OnDeleteEvent` |

## Drawing Areas

### `daTable` (Game Grid)

- **Type:** `Gtk.DrawingArea`
- **DoubleBuffered:** `false` (direct GDK drawing)
- **Events:** `ButtonPressMask`, `ExposeEvent`
- **Rendering:** `GameEngine.DrawTable()` via `Gtk.DotNet.Graphics.FromDrawable(daTable.GdkWindow)`
- **Coordinate System:** Origin (0,0) at top-left; X right, Y down

### `daInfoBar` (Status Bar)

- **Type:** `Gtk.DrawingArea`
- **Events:** `ExposeEvent`
- **Rendering:** `GameEngine.DrawInfoBar()` via `Gtk.DotNet.Graphics.FromDrawable(daInfoBar.GdkWindow)`
- **Layout:** Three equal horizontal thirds (pieces left, face, timer)

## GTK Application Lifecycle

### Initialisation (`Program.cs`)

```csharp
Application.Init();           // GTK# initialisation
MainWindow win = new MainWindow();
win.Show();                   // Show window
Application.Run();            // Main loop (blocks)
```

### Termination

- `OnDeleteEvent` → `Application.Quit()` → exits `Application.Run()`
- `actQuit` → `OnDeleteEvent` → same path

## Dependencies

| Dependency | Used By |
|------------|---------|
| `Gtk.Window`, `Gtk.DrawingArea`, `Gtk.MenuBar`, `Gtk.Action` | `MainWindow` construction |
| `Gdk.Window`, `Gdk.EventMask` | Drawing surface references, event masks |
| `Gtk.DotNet.Graphics` | Bridge to `System.Drawing` for engine |
| `GameEngine` | All game logic delegation |

## Boundary Rules

- **No game logic in `MainWindow`** — only coordinate translation and delegation
- **No direct `Piece`/`GameTable` access** — only via `GameEngine` methods
- **No rendering code** — all rendering in `GameEngine`
- **No timer logic** — timer managed by `GameEngine` via `GLib.Timeout`