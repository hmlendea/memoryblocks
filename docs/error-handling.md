# Error Handling

This document describes error handling strategies, patterns, and gaps.

## Current State

**No formal error handling framework exists.** The codebase uses no try/catch blocks, no exception types, no logging, and no error recovery mechanisms. Exceptions propagate unhandled to the GTK main loop, typically crashing the application.

## Exception Sources

### 1. Array Bounds (Critical)

**Location:** `MainWindow.OnDaTableButtonPressEvent`
```csharp
int x = (int)args.Event.X / tileSize;
int y = (int)args.Event.Y / tileSize;
game.FlipPiece(x, y);  // → GameTable.Pieces[x, y]
```

**Risk:** Click outside drawing area → `x` or `y` ≥ `tableSize` → `IndexOutOfRangeException`

**Mitigation:** None. No bounds checking.

### 2. Resource Loading (Critical)

**Location:** `GameEngine.DrawTable`
```csharp
Bitmap bmp = Gdk.Pixbuf.LoadFromResource(
    "MemoryBlocks.Resources.Pieces.piece" + piece.Value + ".png").ToBitmap();
```

**Risk:** `piece.Value` ≥ 37 → resource not found → `NullReferenceException` or `Exception` from `LoadFromResource`

**Mitigation:** None. `GameTable.piecesMaxValue = 102` but only 37 images exist.

### 3. Null Drawables (High)

**Location:** `GameEngine.DrawTable`, `GameEngine.DrawInfoBar`
```csharp
gfx = Gtk.DotNet.Graphics.FromDrawable(gdkWindowTable);
```

**Risk:** `gdkWindowTable` or `gdkWindowInfoBar` not yet realised → `NullReferenceException`

**Mitigation:** None. Assumes widgets realised before first draw.

### 4. Bitmap Allocation (Medium)

**Location:** `GameEngine.DrawTable` (inside loop)
```csharp
using (Bitmap bmp = ...) { gfx.DrawImage(bmp, rec); }
```

**Risk:** `OutOfMemoryException` under memory pressure

**Mitigation:** `using` ensures disposal, but no retry/fallback.

### 5. Timer Callback (Medium)

**Location:** `GameEngine.TimerTick`
```csharp
return isRunning;  // Return false to stop timer
```

**Risk:** Exception in `DrawInfoBar` → timer stops silently, game appears frozen

**Mitigation:** None.

## Exception Propagation

```
User Click
    │
    ├─► GTK Main Loop
    │       └─► OnDaTableButtonPressEvent
    │               ├─► FlipPiece
    │               │       ├─► CheckFlipped
    │               │       └─► UnflipAllPieces
    │               ├─► DrawTable
    │               │       └─► LoadFromResource → Exception
    │               ├─► DrawInfoBar
    │               └─► ShowDialog
    │
    └─► Unhandled Exception → GTK Main Loop Crash → Application Exit
```

No `Application.ThreadException`, no `AppDomain.UnhandledException`, no `GLib.ExceptionManager`.

## Error Handling Patterns (Absent)

| Pattern | Status | Notes |
|---------|--------|-------|
| Try/Catch | ❌ Not used | Zero occurrences in codebase |
| Custom Exceptions | ❌ Not defined | No `*Exception` classes |
| Result/Option Types | ❌ Not used | No functional error handling |
| Error Codes | ❌ Not used | No return-code conventions |
| Logging | ❌ Not implemented | No log output anywhere |
| User-Facing Errors | ❌ Not shown | Only `ShowMessageDialog` for "Level complete" |
| Recovery | ❌ Not possible | Any exception = crash |

## Error Scenarios and Current Behaviour

| Scenario | Current Behaviour | Desired Behaviour |
|----------|-------------------|-------------------|
| Click outside grid | Crash (`IndexOutOfRangeException`) | Ignore click / show tooltip |
| Missing piece image | Crash (`Exception` from `LoadFromResource`) | Show placeholder / validate at startup |
| Drawing before realise | Crash (`NullReferenceException`) | Defer draw / check `IsRealized` |
| Out of memory | Crash (`OutOfMemoryException`) | Reduce quality / show error dialog |
| Timer exception | Timer stops, game frozen | Log, restart timer, show indicator |
| Invalid grid size | Crash in `GenerateTable` | Validate, clamp to valid range |

## Recommended Error Handling Strategy

### 1. Defensive Programming (Immediate)

Add bounds checking in `OnDaTableButtonPressEvent`:
```csharp
if (x < 0 || x >= game.TableSize || y < 0 || y >= game.TableSize)
    return;  // Ignore out-of-bounds clicks
```

Add null checks for drawables:
```csharp
if (gdkWindowTable == null || gdkWindowInfoBar == null)
    return;  // Skip render until realised
```

### 2. Startup Validation (Immediate)

Validate assets match `piecesMaxValue` in `GameEngine.NewGame`:
```csharp
for (int v = 0; v <= piecesMaxValue; v++)
{
    var resource = $"MemoryBlocks.Resources.Pieces.piece{v}.png";
    if (Gdk.Pixbuf.LoadFromResource(resource) == null)
        throw new InvalidOperationException($"Missing piece image: {resource}");
}
```

### 3. Structured Exception Handling (Short-term)

Define custom exceptions:
```csharp
public class GameException : Exception { ... }
public class AssetMissingException : GameException { ... }
public class InvalidGameStateException : GameException { ... }
```

Wrap critical sections:
```csharp
try
{
    game.FlipPiece(x, y);
    game.DrawTable();
    game.DrawInfoBar();
}
catch (AssetMissingException ex)
{
    ShowMessageDialog("Missing game assets: " + ex.Message, MessageType.Error);
}
catch (Exception ex)
{
    LogError(ex);  // To file/syslog
    ShowMessageDialog("An error occurred: " + ex.Message, MessageType.Error);
}
```

### 4. Logging Infrastructure (Short-term)

Add minimal logging:
```csharp
public static class GameLog
{
    public static void Error(string msg, Exception ex = null) { ... }
    public static void Warn(string msg) { ... }
    public static void Info(string msg) { ... }
}
```

Output to:
- Console (when launched from terminal)
- `~/.local/share/MemoryBlocks/logs/` (rotating files)
- Systemd journal (on Linux)

### 5. Global Exception Handler (Short-term)

```csharp
Application.ThreadException += (sender, e) =>
{
    GameLog.Error("Unhandled UI exception", e.Exception);
    ShowMessageDialog("Unexpected error: " + e.Exception.Message, MessageType.Error);
};

AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    GameLog.Error("Unhandled exception", e.ExceptionObject as Exception);
};
```

## Error Handling in Related Projects

| Project | Approach |
|---------|----------|
| GTK# Applications | Typically use `GLib.ExceptionManager` + try/catch in callbacks |
| MonoGame/XNA | Try/catch in `Update`/`Draw`, custom `GameException` hierarchy |
| Modern .NET | `ILogger`, `ProblemDetails`, middleware pipeline |

## Testing Error Handling

No tests exist. Recommended test cases:

| Test | Expected |
|------|----------|
| Click at (-1, -1) | No crash, no state change |
| Click at (tableSize, tableSize) | No crash, no state change |
| `piece.Value = 37` in `DrawTable` | Graceful error, not crash |
| `gdkWindowTable = null` in `DrawTable` | Skip render, no crash |
| Timer throws in `DrawInfoBar` | Timer continues, error logged |

## Error Handling Debt

| Item | Severity | Effort |
|------|----------|--------|
| Bounds checking on click | Critical | Low |
| Asset validation at startup | Critical | Low |
| Null checks for drawables | High | Low |
| Custom exception types | Medium | Medium |
| Global exception handler | Medium | Low |
| Logging infrastructure | Medium | Medium |
| User-facing error dialogs | Medium | Low |
| Recovery strategies | Low | High |

## Migration Path

1. **Phase 1 (Defensive):** Add bounds/null checks, asset validation — no new types
2. **Phase 2 (Structured):** Define `GameException` hierarchy, wrap callbacks
3. **Phase 3 (Observability):** Add logging, global handlers, error reporting
4. **Phase 4 (Resilience):** Retry logic, fallback rendering, state recovery