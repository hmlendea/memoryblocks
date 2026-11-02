# Ambiguities and Open Questions

This document tracks unresolved design decisions, known ambiguities, and open questions requiring clarification.

## Critical Ambiguities

### A1: Asset/Value Mismatch (Critical)
**Issue:** `GameTable.piecesMaxValue = 102` but only 37 piece images exist (piece0.png–piece36.png).
**Impact:** Values 37–101 cause runtime crash in `DrawTable`.
**Questions:**
- Was `piecesMaxValue` intended to be 37?
- Should `GenerateTable` clamp values to 36?
- Are more piece images planned?
- Why 102 specifically? (2 × 51, but max pairs is 32 for 8×8)

**Current Behaviour:** Works incidentally because max pairs = 32 (8×8), so max value = 31. But any change to board size or algorithm could break.

### A2: Multiple Timers on New Game (High)
**Issue:** Each `NewGame()` creates new `GameEngine` → new `GLib.Timeout` → old timer not removed.
**Impact:** Multiple timer callbacks fire, `gameTime` increments multiple times per second, `DrawInfoBar` called multiple times.
**Questions:**
- Was this intentional?
- Should `GameEngine` implement `IDisposable` to remove timer?
- Should `MainWindow` track and remove old timer?

### A3: No Bounds Checking on Click (High)
**Issue:** `OnDaTableButtonPressEvent` calculates `x, y` from mouse coordinates without validating against `tableSize`.
**Impact:** Click outside drawing area → `IndexOutOfRangeException` → application crash.
**Questions:**
- Should clicks outside be ignored silently?
- Should visual feedback indicate valid area?
- Is drawing area guaranteed to match grid exactly?

### A4: Right-Click Handler Empty (Medium)
**Issue:** `OnDaTableButtonPressEvent` has `else if (args.Event.Button == 3) { }` — empty block.
**Impact:** Right-click does nothing, but handler exists suggesting intent.
**Questions:**
- Was a context menu planned?
- Should right-click unflip?
- Should it be removed to avoid confusion?

## Design Ambiguities

### D1: Piece.Size Field (Low)
**Issue:** `Piece.Size = 48` set in constructor, never read.
**Questions:**
- Legacy from earlier design?
- Intended for variable tile sizes?
- Should be removed?

### D2: GameTable.piecesMaxValue Constant (Medium)
**Issue:** `piecesMaxValue = 102` is `const` but only used as upper bound in `GenerateTable` (which never reaches it).
**Questions:**
- Should it be `readonly` or configurable?
- Should it match actual asset count (37)?
- Is it a placeholder for future expansion?

### D3: Timer Precision Requirements (Low)
**Issue:** `GLib.Timeout` at 1000ms is approximate; `gameTime` counts ticks not wall time.
**Questions:**
- Is second-level precision sufficient?
- Should `DateTime.UtcNow` be used for actual elapsed time?
- Does drift matter for gameplay?

### D4: Modal Dialog Blocks Timer (Medium)
**Issue:** "Level complete" dialog blocks main loop → timer pauses.
**Questions:**
- Is this acceptable?
- Should timer continue in background?
- Should `gameTime` reflect real elapsed time?

### D5: Stetic vs Manual UI (Medium)
**Issue:** UI partially in `gui.stetic` (Stetic designer), partially in `MainWindow.Build()`.
**Questions:**
- Is Stetic still maintained?
- Should UI be migrated to manual code?
- Are there Stetic-specific limitations?

## Architectural Questions

### Q1: Target Framework Migration
**Question:** Should the project migrate to .NET 8+ with GTK 4?
**Considerations:**
- GTK2 EOL, no security updates
- `System.Drawing.Common` Windows-only in .NET Core+
- Significant rewrite required
- Stetic designer not available for GTK3/4

### Q2: Rendering Backend
**Question:** Replace `System.Drawing` + `gtk-dotnet` with Cairo/SkiaSharp?
**Considerations:**
- Cross-platform, hardware-accelerated
- No 32-bit limitation
- Modern API
- Breaking change for all rendering code

### Q3: Configuration System
**Question:** Add runtime configuration (JSON, INI, etc.)?
**Considerations:**
- Currently all hardcoded
- Grid sizes, colours, timer interval could be configurable
- Low priority for casual game

### Q4: Persistence / Save Games
**Question:** Add game state persistence?
**Considerations:**
- Currently zero persistence
- Would enable resume, statistics, high scores
- Requires serialisation, file I/O, migration strategy

### Q5: Testing Strategy
**Question:** Add unit/integration tests?
**Considerations:**
- Currently zero tests
- `GameTable`/`Piece` are testable (no deps)
- `GameEngine` needs drawing abstraction for testing
- See [Testing](testing.md) for detailed plan

### Q6: Internationalisation
**Question:** Add i18n support?
**Considerations:**
- All strings hardcoded English
- GTK# supports gettext
- Low priority but good practice

### Q7: Accessibility
**Question:** Improve a11y (screen readers, keyboard nav, high contrast)?
**Considerations:**
- Currently mouse-only, hardcoded colours
- GTK/ATK provides infrastructure
- Would broaden audience

## Technical Debt Questions

### TD1: GameEngine Disposal
**Question:** Should `GameEngine` implement `IDisposable`?
**Current:** No disposal, timer leaks on `NewGame`.
**Fix:** Store timer ID, `GLib.Source.Remove(timerId)` in `Dispose()`.

### TD2: Bitmap Caching
**Question:** Cache piece `Bitmap` objects instead of recreating per frame?
**Current:** New `Bitmap` per flipped tile per `DrawTable` call.
**Fix:** Load all 37 images once in `NewGame`, store in `Dictionary<int, Bitmap>`.

### TD3: Random Seed for Reproducibility
**Question:** Add seed parameter to `GameTable` for deterministic boards?
**Current:** `new Random()` — non-deterministic.
**Fix:** `public GameTable(int size, int? seed = null)` for testing/replay.

### TD4: Exception Handling
**Question:** Add global exception handler and user-friendly error dialogs?
**Current:** Unhandled exceptions crash application.
**Fix:** `Application.ThreadException`, `AppDomain.UnhandledException` handlers.

## Priority Matrix

| ID | Category | Severity | Effort | Priority |
|----|----------|----------|--------|----------|
| A1 | Critical Bug | Critical | Low | **P0** |
| A2 | Bug | High | Low | **P0** |
| A3 | Bug | High | Low | **P0** |
| A4 | Code Quality | Medium | Low | P2 |
| D1 | Code Quality | Low | Low | P3 |
| D2 | Design | Medium | Low | P2 |
| D3 | Design | Low | Low | P3 |
| D4 | UX | Medium | Low | P2 |
| D5 | Architecture | Medium | Medium | P2 |
| Q1 | Architecture | High | High | P1 (strategic) |
| Q2 | Architecture | High | High | P1 (strategic) |
| Q3 | Feature | Low | Medium | P3 |
| Q4 | Feature | Medium | Medium | P2 |
| Q5 | Quality | High | Medium | **P0** |
| Q6 | Feature | Low | Medium | P3 |
| Q7 | Feature | Medium | Medium | P2 |
| TD1 | Tech Debt | High | Low | **P0** |
| TD2 | Performance | Medium | Low | P1 |
| TD3 | Testability | Medium | Low | P1 |
| TD4 | Reliability | High | Medium | **P0** |

## Resolution Process

For each item:
1. **Assign owner** — who decides?
2. **Gather evidence** — code review, testing, user feedback
3. **Propose options** — with trade-offs
4. **Decide** — document decision in [Design Decisions](design-decisions.md)
5. **Implement** — create issue, write code, test
6. **Verify** — confirm fix, update docs

## Tracking

| ID | Status | Owner | Decision Link |
|----|--------|-------|---------------|
| A1 | Open | — | — |
| A2 | Open | — | — |
| A3 | Open | — | — |
| A4 | Open | — | — |
| D1 | Open | — | — |
| D2 | Open | — | — |
| D3 | Open | — | — |
| D4 | Open | — | — |
| D5 | Open | — | — |
| Q1 | Open | — | — |
| Q2 | Open | — | — |
| Q3 | Open | — | — |
| Q4 | Open | — | — |
| Q5 | Open | — | — |
| Q6 | Open | — | — |
| Q7 | Open | — | — |
| TD1 | Open | — | — |
| TD2 | Open | — | — |
| TD3 | Open | — | — |
| TD4 | Open | — | — |