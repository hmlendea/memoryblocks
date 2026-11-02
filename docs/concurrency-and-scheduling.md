# Concurrency and Scheduling

This document describes the concurrency model, threading, and scheduling.

## Concurrency Model

**Single-threaded, event-driven.** The entire application runs on the GTK main loop thread. No background threads, no thread pool, no async/await, no Task Parallel Library.

## Thread Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Process                                 │
│  ┌─────────────────────────────────────────────────────┐    │
│  │                  Main Thread                         │    │
│  │  ┌─────────────────────────────────────────────┐    │    │
│  │  │           GTK Main Loop (GLib.MainLoop)      │    │    │
│  │  │                                              │    │    │
│  │  │  ┌─────────────┐  ┌─────────────────────┐   │    │    │
│  │  │  │ Event Queue │  │ Timeout Sources     │   │    │    │
│  │  │  │             │  │                     │   │    │    │
│  │  │  │ • ButtonPress│  │ • TimerTick (1000ms)│   │    │    │
│  │  │  │ • Expose    │  │ • Idle callbacks    │   │    │    │
│  │  │  │ • KeyPress  │  │                     │   │    │    │
│  │  │  │ • Configure │  │                     │   │    │    │
│  │  │  │ • Delete    │  │                     │   │    │    │
│  │  │  └─────────────┘  └─────────────────────┘   │    │    │
│  │  │         │                  │                 │    │    │
│  │  │         ▼                  ▼                 │    │    │
│  │  │  ┌─────────────────────────────────────┐    │    │    │
│  │  │  │         Dispatch & Execute          │    │    │    │
│  │  │  │  • OnDaTableButtonPressEvent        │    │    │    │
│  │  │  │  • OnDeleteEvent                    │    │    │    │
│  │  │  │  • OnActEasy/Moderate/HardActivated │    │    │    │
│  │  │  │  • OnActExitActivated               │    │    │    │
│  │  │  │  • TimerTick callback               │    │    │    │
│  │  │  └─────────────────────────────────────┘    │    │    │
│  │  └─────────────────────────────────────────────┘    │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

## Scheduling Mechanisms

### 1. GTK Event Loop (Primary)

**Mechanism:** `GLib.MainLoop` / `Application.Run()`

**Event Sources:**
- X11/Wayland window system events → GDK → GTK signals
- `GLib.Timeout` → periodic callbacks
- `GLib.Idle` → callbacks when loop idle

**Dispatch:** FIFO per priority level; events processed sequentially

### 2. Timer (GameEngine.TimerTick)

**Registration:**
```csharp
// GameEngine constructor
GLib.Timeout.Add(1000, new GLib.TimeoutHandler(TimerTick));
```

**Callback Signature:**
```csharp
private bool TimerTick()
{
    // Return true to continue, false to remove
}
```

**Behaviour:**
- Fires approximately every 1000ms (not guaranteed real-time)
- Runs on main loop thread
- If callback throws, timer stops (returns false implicitly)
- If callback takes > 1000ms, next tick delayed (no queuing)

### 3. Idle Callbacks (Not Used)

Available but not used:
```csharp
GLib.Idle.Add(() => { /* work */ return false; });
```

## Concurrency Guarantees

| Property | Guarantee | Mechanism |
|----------|-----------|-----------|
| **Mutual Exclusion** | All game state accessed from single thread | Single-threaded main loop |
| **No Data Races** | No concurrent access to `GameEngine`, `GameTable`, `Piece` | No background threads |
| **No Deadlocks** | No locks, no blocking waits | No synchronization primitives |
| **No Starvation** | Timer and events share loop fairly | GLib main loop scheduling |
| **Deterministic Order** | Events processed in arrival order | FIFO event queue |

## What Runs Where

| Code | Thread | Notes |
|------|--------|-------|
| `Program.Main` | Main | Entry point, `Application.Init()`, `Application.Run()` |
| `MainWindow` constructor | Main | UI construction, `NewGame(6)` |
| `GameEngine` constructor | Main | `GameTable` creation, timer registration |
| `GameTable.GenerateTable` | Main | Random board generation |
| `OnDaTableButtonPressEvent` | Main | User click → `FlipPiece` → `DrawTable` → `DrawInfoBar` |
| `TimerTick` | Main | `gameTime++`, `DrawInfoBar`, completion check |
| `DrawTable` / `DrawInfoBar` | Main | `System.Drawing` via `Gtk.DotNet.Graphics` |
| `ShowMessageDialog` | Main | Modal dialog blocks main loop |
| `Application.Quit` | Main | Terminates main loop |

## No Background Work

| Operation | Current | Alternative (if needed) |
|-----------|---------|------------------------|
| Image loading | Synchronous in `DrawTable` | Pre-load/cache in `NewGame` |
| Resource loading | Synchronous (`LoadFromResource`) | Same (embedded, fast) |
| File I/O | None | `GLib.Idle` + async if added |
| Network | None | `GLib.Idle` + async if added |
| Computation | Trivial (O(n²) n≤8) | `Task.Run` + `GLib.Idle` for completion |

## Timer Precision

| Aspect | Value |
|--------|-------|
| Nominal interval | 1000ms |
| Actual interval | 1000ms + event loop latency |
| Jitter | Typically < 10ms, unbounded under load |
| Drift | Accumulates; `gameTime` counts ticks, not wall time |
| Pause during dialog | Yes — main loop blocked by modal dialog |

## Concurrency Anti-Patterns (Avoided)

| Anti-Pattern | Status |
|--------------|--------|
| `Thread.Start` | ❌ Not used |
| `ThreadPool.QueueUserWorkItem` | ❌ Not used |
| `Task.Run` | ❌ Not used |
| `async`/`await` | ❌ Not used (targets .NET 4.5) |
| `BackgroundWorker` | ❌ Not used |
| `lock` / `Monitor` | ❌ Not used |
| `Mutex` / `Semaphore` | ❌ Not used |
| `ConcurrentQueue` | ❌ Not used |
| `Interlocked` | ❌ Not used |

## If Concurrency Were Added

### Required Changes

1. **Thread-safe GameEngine**
```csharp
private readonly object _gate = new object();

public void FlipPiece(int x, int y)
{
    lock (_gate) { /* logic */ }
}
```

2. **Marshal UI Updates**
```csharp
GLib.Idle.Add(() => {
    DrawTable();
    DrawInfoBar();
    return false;
});
```

3. **Timer on Background Thread**
```csharp
// System.Threading.Timer instead of GLib.Timeout
var timer = new Timer(_ => {
    lock (_gate) { gameTime++; }
    GLib.Idle.Add(() => { DrawInfoBar(); return false; });
}, null, 0, 1000);
```

4. **Immutable Snapshots for Rendering**
```csharp
public GameState GetSnapshot()
{
    lock (_gate) { return new GameState(this); }
}
```

### Not Recommended For This Project

- Scope too small to benefit
- Adds complexity, bugs, testing burden
- GTK# not thread-safe for widget access
- Current performance more than adequate

## Scheduling-Related Bugs (Potential)

| Bug | Cause | Symptom |
|-----|-------|---------|
| Timer drift | `gameTime` counts ticks, not seconds | Clock runs slow under load |
| Timer stops on exception | `TimerTick` throws → returns false | Game clock freezes |
| Modal dialog blocks timer | `ShowMessageDialog` runs nested main loop | Timer pauses during "Level complete" |
| Rapid expose events | Window resize → many `DrawTable` calls | CPU spike, flicker |
| No frame rate limiting | `DrawTable` called on every expose | Unbounded render rate |

## Mitigations (If Needed)

| Issue | Fix |
|-------|-----|
| Timer drift | Use `DateTime.UtcNow` for actual elapsed time |
| Timer exception | Wrap `TimerTick` body in try/catch |
| Dialog blocks timer | Acceptable for this game type |
| Expose storm | Throttle `DrawTable` with minimum interval |
| Render rate | Not needed — trivial rendering |

## Summary

- **Model:** Single-threaded event loop
- **Scheduler:** GLib main loop (priority-based, FIFO)
- **Timer:** `GLib.Timeout` at 1000ms on main thread
- **Synchronization:** None needed
- **Scalability:** Not a concern (n ≤ 8, trivial compute)
- **Migration path:** Would require significant architecture change