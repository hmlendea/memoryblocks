# Logging

This document describes the logging infrastructure, current state, and recommendations.

## Current State

**No logging infrastructure exists.** The codebase produces zero log output. There are no log statements, no logging framework, no log files, no structured logging, and no diagnostic output of any kind.

## Logging in Codebase

| Location | Logging | Notes |
|----------|---------|-------|
| `Program.cs` | ❌ None | Entry point, no startup/shutdown logs |
| `MainWindow.cs` | ❌ None | UI events, no click/render logs |
| `GameEngine.cs` | ❌ None | Game logic, no state change logs |
| `GameTable.cs` | ❌ None | Board generation, no debug logs |
| `Piece.cs` | ❌ None | Model, no logs |
| `Extensions.cs` | ❌ None | Helpers, no logs |

## What Should Be Logged

### Startup/Shutdown
- Application start (version, platform, runtime)
- GTK initialisation success/failure
- Main window construction
- Resource loading (piece images count)
- Application exit (clean vs crash)

### Gameplay Events
- New game started (grid size, seed if deterministic)
- Tile flipped (position, value, flip count)
- Match found (positions, value, pieces remaining)
- Match failed (positions, values)
- Level completed (time, moves/flips)
- Difficulty changed

### Errors/Exceptions
- All unhandled exceptions (full stack trace)
- Resource load failures (missing piece images)
- Null drawable access
- Array bounds violations
- Out of memory

### Performance/Diagnostics
- Frame render time (DrawTable duration)
- Timer tick latency
- Memory allocations (Bitmap count)
- GC pressure indicators

## Recommended Logging Framework

### Option 1: Minimal Custom Logger (Zero Dependencies)

```csharp
// Utils/Logger.cs
using System;
using System.IO;

namespace MemoryBlocks.Utils
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MemoryBlocks", "logs", $"memoryblocks-{DateTime.Now:yyyyMMdd}.log");

        private static readonly object Lock = new object();
        private static bool _initialised;

        public static void Initialise()
        {
            if (_initialised) return;
            var dir = Path.GetDirectoryName(LogPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            _initialised = true;
            Info($"=== MemoryBlocks started at {DateTime.Now} ===");
        }

        public static void Info(string msg) => Write("INFO", msg);
        public static void Warn(string msg) => Write("WARN", msg);
        public static void Error(string msg, Exception ex = null) => Write("ERROR", msg + (ex != null ? " | " + ex : ""));
        public static void Debug(string msg) => Write("DEBUG", msg);

        private static void Write(string level, string msg)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {msg}";
            lock (Lock)
            {
                File.AppendAllText(LogPath, line + Environment.NewLine);
            }
            #if DEBUG
            Console.WriteLine(line);
            #endif
        }
    }
}
```

Usage:
```csharp
// Program.cs
Logger.Initialise();

// GameEngine.cs
Logger.Info($"New game: {tableSize}x{tableSize}");
Logger.Debug($"FlipPiece({x},{y}) value={piece.Value} flips={flips}");
Logger.Error("Failed to load piece image", ex);
```

### Option 2: Serilog (Structured, Extensible)

```xml
<!-- Add to MemoryBlocks.csproj -->
<PackageReference Include="Serilog" Version="2.12.0" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="4.1.0" />
```

```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MemoryBlocks", "logs", "memoryblocks-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

Log.Information("MemoryBlocks started");
```

### Option 3: Microsoft.Extensions.Logging (Modern .NET Standard)

```xml
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.File" Version="3.0.0" />  // Community
```

## Log Levels

| Level | Use Case | Examples |
|-------|----------|----------|
| **Debug** | Detailed diagnostic | Render timing, coordinate calculations, resource paths |
| **Info** | General operational | Game start/end, level complete, difficulty change |
| **Warn** | Recoverable issues | Missing optional asset, deprecated API use, near-limits |
| **Error** | Failures requiring attention | Exceptions, missing required assets, null drawables |
| **Critical** | Application crash imminent | Unhandled exception, out of memory, GTK init failure |

## Structured Logging Fields

Recommended key-value pairs for structured logs:

| Field | Description | Example |
|-------|-------------|---------|
| `timestamp` | ISO 8601 | `2024-01-15T10:30:45.123Z` |
| `level` | Log level | `Information` |
| `message` | Human-readable | `Tile flipped at (2,3)` |
| `eventType` | Categorisation | `Gameplay`, `Rendering`, `Error`, `Startup` |
| `gridSize` | Current board | `6` |
| `pieceValue` | Tile value | `12` |
| `position` | Board coordinates | `{"x":2,"y":3}` |
| `flips` | Current flip count | `1` |
| `piecesLeft` | Remaining pairs | `17` |
| `gameTime` | Seconds elapsed | `45` |
| `durationMs` | Operation timing | `12` |
| `exception` | Serialized exception | `{type, message, stackTrace}` |

## Log Output Destinations

### Development
- Console (stdout/stderr) — immediate visibility
- File: `~/.local/share/MemoryBlocks/logs/memoryblocks-YYYYMMDD.log`

### Production (Linux)
- Systemd journal (via `Console` sink when run as service)
- Rotating files in `XDG_STATE_HOME/MemoryBlocks/logs/`
- Optional: syslog forwarder

### Production (Windows)
- Event Log (via `EventLog` sink)
- Rotating files in `%LOCALAPPDATA%\MemoryBlocks\logs\`

## Log Retention

| Environment | Retention | Max Size | Rotation |
|-------------|-----------|----------|----------|
| Development | 7 days | 10 MB/file | Daily |
| Production | 30 days | 50 MB/file | Daily |
| CI/CD | 1 run | 100 MB | Per run |

## Integration Points

### Program.cs
```csharp
Logger.Initialise();
Logger.Info($"MemoryBlocks v{Assembly.GetExecutingAssembly().GetName().Version} starting");
Logger.Info($"Runtime: {Environment.Version}, OS: {Environment.OSVersion}");
Logger.Info($"GTK: {Gtk.Global.GtkVersion}");
```

### MainWindow.cs
```csharp
Logger.Debug($"MainWindow constructed, gridSize={gridSize}");
Logger.Info($"New game started: {gridSize}x{gridSize}");

protected virtual void OnDaTableButtonPressEvent(...)
{
    Logger.Debug($"Click at ({args.Event.X},{args.Event.Y}) → tile ({x},{y})");
    // ...
}
```

### GameEngine.cs
```csharp
public void FlipPiece(int x, int y)
{
    Logger.Debug($"FlipPiece({x},{y}) flips={flips} piecesLeft={piecesLeft}");
    // ...
    if (match)
        Logger.Info($"Match found: ({flip1i},{flip1j}) and ({i},{j}) value={val1} piecesLeft={piecesLeft}");
    else
        Logger.Debug($"No match: ({flip1i},{flip1j})={val1} vs ({i},{j})={val2}");
}

private bool TimerTick()
{
    var sw = Stopwatch.StartNew();
    // ...
    Logger.Debug($"TimerTick: gameTime={gameTime} renderMs={sw.ElapsedMilliseconds}");
}
```

### Resource Loading
```csharp
var resourceName = $"MemoryBlocks.Resources.Pieces.piece{piece.Value}.png";
var pixbuf = Gdk.Pixbuf.LoadFromResource(resourceName);
if (pixbuf == null)
{
    Logger.Error($"Missing piece resource: {resourceName}");
    // Fallback or throw
}
```

## Log Analysis Queries

### Common Investigations

```bash
# Game sessions
grep "New game started" memoryblocks-20240115.log

# Errors
grep "ERROR" memoryblocks-20240115.log

# Performance
grep "renderMs=" memoryblocks-20240115.log | awk '{print $NF}' | sort -n

# Match rate
grep "Match found" memoryblocks-20240115.log | wc -l
grep "No match" memoryblocks-20240115.log | wc -l
```

## Current Gaps

| Gap | Impact | Effort to Fix |
|-----|--------|---------------|
| No logging at all | Zero observability | Low (custom) / Medium (Serilog) |
| No error capture | Crashes leave no trace | Low (global handler) |
| No performance data | Cannot diagnose slowness | Low (Stopwatch in render) |
| No structured fields | Hard to query/analyse | Medium (structured logger) |
| No log rotation | Disk growth | Low (built into sinks) |
| No correlation IDs | Cannot trace request flow | Medium (ambient context) |

## Migration Checklist

- [ ] Choose logging approach (custom vs Serilog vs MEL)
- [ ] Add logger initialisation in `Program.cs`
- [ ] Add startup/shutdown logging
- [ ] Add gameplay event logging in `GameEngine`
- [ ] Add click/render logging in `MainWindow`
- [ ] Add resource validation logging
- [ ] Add global exception handler with logging
- [ ] Configure file sink with rotation
- [ ] Verify log output in development
- [ ] Document log location for users