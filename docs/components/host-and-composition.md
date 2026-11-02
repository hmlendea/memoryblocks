# Host and Composition

This document describes the application entry point and composition root.

## Program.cs

**File:** `MemoryBlocks/Program.cs`
**Namespace:** `MemoryBlocks`
**Class:** `MainClass`

### Main Method

```csharp
public static void Main(string[] args)
{
    Application.Init();           // Initialise GTK# application context
    MainWindow win = new MainWindow();  // Construct main window (triggers composition)
    win.Show();                   // Display window
    Application.Run();            // Start GTK main event loop (blocks)
}
```

### Responsibilities

1. **GTK# Initialisation** — `Application.Init()` sets up the GTK type system, thread awareness, and event loop infrastructure
2. **Composition Root** — Creates `MainWindow`, which transitively constructs the entire object graph
3. **Main Loop** — `Application.Run()` blocks the main thread, dispatching GTK events until `Application.Quit()`

### No Dependency Injection

- No DI container, no service registration, no configuration binding
- Object graph constructed imperatively in `MainWindow` constructor
- `GameEngine` instantiated directly with `new GameEngine(6)`
- `Gdk.Window` references assigned via property setters after construction

## MainWindow as Composition Root

**File:** `MemoryBlocks/MainWindow.cs`
**Constructor:** `public MainWindow() : base(Gtk.WindowType.Toplevel)`

### Object Graph Construction

```
MainWindow()
├── Build()                                    // Stetic: creates all GTK widgets
├── daTable.DoubleBuffered = false
├── game = new GameEngine(6)                   // Core domain service
├── game.GdkWindowTable = daTable.GdkWindow    // Inject drawing surface
├── game.GdkWindowInfoBar = daInfoBar.GdkWindow
├── game.NewGame(6)                            // Start initial session
├── daTable.ExposeEvent += DrawTable delegate
├── daInfoBar.ExposeEvent += DrawInfoBar delegate
├── daTable.AddEvents(ButtonPressMask)
└── (Menu action handlers wired in Build())
```

### Dependencies Created

| Dependency | Lifetime | Injection Method |
|------------|----------|------------------|
| `GameEngine` | Application lifetime (owned by `MainWindow`) | Direct `new` in constructor |
| `Gdk.Window` (table) | Bound to `daTable` widget lifetime | Property setter after `Build()` |
| `Gdk.Window` (info bar) | Bound to `daInfoBar` widget lifetime | Property setter after `Build()` |
| `GameTable` | Per-session (owned by `GameEngine`) | Created inside `GameEngine.NewGame()` |
| `Piece[,]` | Per-session (owned by `GameTable`) | Created inside `GameTable` constructor |

### No Abstractions

- No interfaces for `GameEngine`, `GameTable`, or `Piece`
- No factory, builder, or provider patterns
- Concrete types used throughout
- Testing would require subclassing or wrapper (not currently supported)

## GameEngine Construction

**File:** `MemoryBlocks/Game/GameEngine.cs`
**Constructor:** `public GameEngine(int tableSize)`

### Initialisation

```csharp
public GameEngine(int tableSize)
{
    backgroundColor = Color.GhostWhite;
    primaryColor = Color.CornflowerBlue;
    secondaryColor = Color.LightBlue;

    GLib.Timeout.Add(1000, new GLib.TimeoutHandler(TimerTick));
}
```

### Responsibilities

1. **Default Colours** — Hardcoded `System.Drawing.Color` values
2. **Timer Registration** — `GLib.Timeout.Add` registers `TimerTick` on GTK main loop
3. **No GameTable Yet** — `GameTable` created lazily in `NewGame()`

### TimerTick Callback

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

- Runs on GTK main loop thread (safe for UI calls)
- 1000ms interval (approximate)
- Updates `gameTime` and triggers info bar redraw
- Self-terminates when game complete

## GameTable Construction

**File:** `MemoryBlocks/Game/GameTable.cs`
**Constructor:** `public GameTable(int tableSize)`

### Initialisation

```csharp
public GameTable(int tableSize)
{
    this.tableSize = tableSize;
    InitializePieces();
    GenerateTable();
}
```

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

- Allocates `Piece[,]` matrix
- Creates `Piece` for every cell
- Sets position coordinates

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
        while (val == -1 || valuesUsed.Contains(val))
            val = rnd.Next(0, piecesMaxValue);  // piecesMaxValue = 102
        valuesUsed.Add(val);

        while (placed < 2)
        {
            int x = rnd.Next(0, TableSize);
            int y = rnd.Next(0, TableSize);
            if (pieces[x, y].Value < 0)
            {
                pieces[x, y].Value = val;
                placed += 1;
            }
        }
    }
}
```

- Generates `piecesCount` unique random values (0–101)
- Places each value in exactly two random empty cells
- `piecesMaxValue = 102` (constant, not configurable)

## Piece Construction

**File:** `MemoryBlocks/Game/Piece.cs`
**Constructor:** `public Piece()`

### Default State

```csharp
public Piece()
{
    size = 48;
    posX = 0;
    posY = 0;
    isFlipped = false;
    isCleared = false;
    value = -1;  // Uninitialised marker
}
```

- `size = 48` (legacy, not used in rendering — tile size computed dynamically)
- `value = -1` indicates uninitialised (used by `GenerateTable` to find empty cells)

## Composition Summary

```
MainClass.Main()
  └─ Application.Init()
  └─ new MainWindow()
        └─ Build() → GTK widget tree
        └─ new GameEngine(6)
              └─ GLib.Timeout.Add(TimerTick)
              └─ (colours initialised)
        └─ game.GdkWindowTable = daTable.GdkWindow
        └─ game.GdkWindowInfoBar = daInfoBar.GdkWindow
        └─ game.NewGame(6)
              └─ new GameTable(6)
                    └─ InitializePieces() → new Piece[6,6]
                    └─ GenerateTable() → random pairs
              └─ Reset counters
              └─ DrawTable(), DrawInfoBar()
  └─ win.Show()
  └─ Application.Run() → event loop
```

## Configuration Points (Hardcoded)

| Value | Location | Change Requires |
|-------|----------|-----------------|
| Default grid size (6) | `MainWindow` constructor, `Program.cs` | Recompile |
| Timer interval (1000ms) | `GameEngine` constructor | Recompile |
| Colours (GhostWhite, CornflowerBlue, LightBlue) | `GameEngine` constructor | Recompile |
| Tile spacing (2px) | `GameEngine.NewGame()` | Recompile |
| Max piece value (102) | `GameTable.piecesMaxValue` | Recompile |
| Piece size (48) | `Piece` constructor | Recompile (unused) |
| Difficulty grid sizes (4, 6, 8) | `MainWindow` menu handlers | Recompile |

## No External Configuration

- No `app.config`, `appsettings.json`, `global.json`, or environment variable usage
- All configuration is compile-time constants
- No feature flags, no runtime toggles