# Design Decisions

This document records key architectural and design choices with their rationale.

## 1. GTK# 2.12 / GTK2 Targeting

**Decision:** Build against GTK# 2.12 (GTK2) rather than GTK3/GTK4.

**Rationale:**
- Mono's GTK# bindings for GTK2 are mature and stable
- Simpler deployment on Linux distributions with GTK2 runtime
- Existing codebase and Stetic GUI designer integration
- Lower memory footprint than GTK3/GTK4

**Trade-offs:**
- GTK2 is end-of-life; no security updates upstream
- Limited modern UI capabilities (no CSS styling, limited accessibility)
- Migration path to GTK3/GTK4 would require significant UI rewrite

## 2. MVP/MVC Hybrid Architecture

**Decision:** Use event-driven MVP/MVC hybrid with `MainWindow` as View, `GameEngine` as Presenter/Controller, and `GameTable`/`Piece` as Model.

**Rationale:**
- Clear separation between UI concerns (GTK widgets, event handling) and game logic
- `GameEngine` and domain models are testable in isolation (though no tests exist yet)
- `GameEngine` operates on generic `Gdk.Window` — not tied to specific widget hierarchy
- Familiar pattern for desktop GUI applications

**Trade-offs:**
- `GameEngine` mixes presentation (rendering) with game logic
- `MainWindow` directly calls `GameEngine` methods rather than using commands/events
- No formal interface for `GameEngine` (no `IGameEngine`)

## 3. System.Drawing / GDI+ Rendering via gtk-dotnet

**Decision:** Use `System.Drawing.Graphics` via `Gtk.DotNet.Graphics.FromDrawable()` for all rendering instead of native Cairo.

**Rationale:**
- `System.Drawing` API familiar to .NET developers
- GDI+ operations (DrawImage, FillRectangle, DrawString) map directly to game rendering needs
- `gtk-dotnet` bridge handles GDK surface interop
- Existing codebase already used this approach

**Trade-offs:**
- `System.Drawing` on Linux via Mono uses libgdiplus (not fully compatible with Windows GDI+)
- Performance overhead of bridge vs native Cairo
- libgdiplus may have rendering differences vs Windows
- `System.Drawing.Common` only supports Windows in .NET Core/.NET 5+; this codebase stays on .NET Framework 4.5 / Mono

## 4. In-Memory Only State (No Persistence)

**Decision:** No game state persistence to disk; all state exists only in memory during session.

**Rationale:**
- Simple puzzle game; persistence adds complexity without clear user value
- No user accounts, profiles, or progression systems
- Fast startup/shutdown; no file I/O errors possible
- Aligns with privacy-first design (no personal data stored)

**Trade-offs:**
- Game progress lost on application close or crash
- No high scores, statistics, or resume capability
- Cannot implement "continue game" feature

## 5. Embedded Resources for Assets

**Decision:** Compile all piece images (37 PNGs) and application icon as embedded assembly resources.

**Rationale:**
- Single-file deployment (no external asset files to manage)
- Resources loaded via `Gdk.Pixbuf.LoadFromResource()` at render time
- Version-controlled with source code
- No file system path resolution issues

**Trade-offs:**
- Assembly size increases (~37 small PNGs)
- Adding new piece graphics requires recompilation
- Resource naming convention (`piece{ID}.png`) couples `GameTable` value range to asset count

## 6. Random Pair Generation Algorithm

**Decision:** `GameTable.GenerateTable()` uses `Random` with value tracking to place matching pairs.

**Algorithm:**
```
for each pair (0 to piecesCount-1):
  pick unique random value (0-101)
  place value in two random empty cells
```

**Rationale:**
- Simple, deterministic enough for casual game
- Ensures exactly two of each value
- `piecesMaxValue = 102` provides large value space vs 18 pairs max (8×8 = 64 tiles / 2 = 32 pairs)

**Trade-offs:**
- `Random` not cryptographically secure (acceptable for game)
- Value range (102) exceeds actual asset count (37) — values 37-101 would fail to load images
- No guarantee of solvable board configuration (all memory games are solvable by definition)
- Potential infinite loop if `piecesMaxValue` too small for grid size (not an issue here)

## 7. Timer via GLib.Timeout

**Decision:** Use `GLib.Timeout.Add(1000, TimerTick)` for game timer instead of `System.Timers.Timer` or `Thread.Sleep`.

**Rationale:**
- Executes on GTK main loop thread — safe for UI updates (`DrawInfoBar()`)
- No cross-thread marshaling needed
- Integrates with GTK event loop naturally
- Single-threaded execution model preserved

**Trade-offs:**
- Timer resolution limited to ~1 second (acceptable for game timer)
- Timer pauses if main loop blocked (e.g., modal dialog open)
- Not suitable for high-precision timing

## 8. No Automated Test Suite

**Decision:** No unit tests, integration tests, or automated test infrastructure.

**Rationale:**
- Small codebase (~500 lines of game logic)
- Manual verification sufficient for current scope
- GTK# GUI testing requires display server (complex in CI)
- Project appears to be learning/personal project

**Trade-offs:**
- No regression protection for refactoring
- No CI/CD pipeline validation
- Bug fixes lack regression tests
- Difficult to verify behaviour changes safely

## 9. Single-Threaded Execution Model

**Decision:** All code executes on GTK main thread; no background threads, `Task`, `async/await`, or thread pool usage.

**Rationale:**
- GTK# is not thread-safe; all widget access must be on main thread
- Simplifies reasoning about state mutations
- No locking, race conditions, or synchronization needed
- `GLib.Timeout` and event handlers all run on main loop

**Trade-offs:**
- Long-running operations would freeze UI (none exist currently)
- Cannot leverage multi-core for computation (not needed)
- Timer and rendering share main loop time slice

## 10. Difficulty Levels as Grid Sizes Only

**Decision:** Three difficulty levels map directly to grid sizes: Easy=4×4, Moderate=6×6, Hard=8×8.

**Rationale:**
- Simple, intuitive difficulty progression
- No additional parameters (time limits, flip limits, etc.)
- Grid size directly affects memory load and game duration

**Trade-offs:**
- No fine-grained difficulty tuning
- 4×4 may be too easy; 8×8 may be too hard for some players
- No "custom" grid size option