# Testing

This document describes the testing strategy, current state, and recommendations.

## Current State

**No tests exist.** The codebase has:
- No test project in the solution
- No unit tests
- No integration tests
- No UI tests
- No test framework configured
- No CI test execution

## Test Project Structure (Recommended)

```
MemoryBlocks.Tests/
├── MemoryBlocks.Tests.csproj
├── Unit/
│   ├── GameTableTests.cs
│   ├── PieceTests.cs
│   └── GameEngineTests.cs
├── Integration/
│   └── GameFlowTests.cs
├── TestBase.cs
└── TestData/
    └── (none needed)
```

### Test Project Configuration

```xml
<!-- MemoryBlocks.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="NUnit" Version="3.13.3" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.2.1" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.5.0" />
    <PackageReference Include="Moq" Version="4.18.4" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\MemoryBlocks\MemoryBlocks.csproj" />
  </ItemGroup>
</Project>
```

## What to Test

### Unit Tests — GameTable

| Test Case | Description |
|-----------|-------------|
| `GenerateTable_CreatesCorrectSize` | 4×4, 6×6, 8×8 boards have correct dimensions |
| `GenerateTable_CreatesPairs` | Every value appears exactly twice |
| `GenerateTable_ValuesInRange` | All values 0 ≤ v < piecesMaxValue |
| `GenerateTable_DifferentSeedsProduceDifferentBoards` | Non-deterministic (or seeded) |
| `InitializePieces_SetsPositions` | Each piece has correct PositionX/Y |
| `PiecesMaxValue_Constant` | Verify value is 102 |

### Unit Tests — Piece

| Test Case | Description |
|-----------|-------------|
| `Constructor_Defaults` | IsFlipped=false, IsCleared=false, Size=48 |
| `Properties_CanBeSet` | All properties writable |
| `IsCleared_ImpliesNotFlipped` | Domain invariant (enforced by GameEngine) |

### Unit Tests — GameEngine

| Test Case | Description |
|-----------|-------------|
| `NewGame_InitialisesState` | isRunning=true, gameTime=0, flips=0, piecesLeft=N/2 |
| `FlipPiece_FirstFlip_SetsFlipped` | flips=1, piece.IsFlipped=true |
| `FlipPiece_SecondFlipMatch_ClearsBoth` | flips=0, piecesLeft--, both IsCleared=true |
| `FlipPiece_SecondFlipNoMatch_KeepsFlipped` | flips=2, both IsFlipped=true |
| `FlipPiece_ThirdFlipAfterNoMatch_UnflipsAllThenFlipsNew` | flips=1, only new piece flipped |
| `FlipPiece_OnClearedTile_NoOp` | State unchanged |
| `FlipPiece_OnFlippedTile_Unflips` | flips=0, piece.IsFlipped=false |
| `FlipPiece_OutOfBounds_Throws` | IndexOutOfRangeException (current) or handled |
| `CheckFlipped_FindsTwoFlipped` | Correctly identifies flipped pair |
| `UnflipAllPieces_ResetsAll` | All IsFlipped=false, flips=0 |
| `Completed_WhenPiecesLeftZero` | Returns true |
| `TimerTick_IncrementsTime` | gameTime++ while running |
| `TimerTick_StopsWhenComplete` | Returns false, isRunning=false |

### Integration Tests — Game Flow

| Test Case | Description |
|-----------|-------------|
| `FullGame_Easy_Wins` | 4×4 board, simulate perfect play, verify completion |
| `FullGame_Moderate_Wins` | 6×6 board |
| `FullGame_Hard_Wins` | 8×8 board |
| `NewGame_ResetsState` | After completion, NewGame creates fresh board |
| `DifficultyChange_ChangesGridSize` | Menu actions produce correct tableSize |

### UI Tests (If Feasible)

| Test Case | Description |
|-----------|-------------|
| `MainWindow_Constructs` | No exception |
| `MenuItems_Exist` | Easy/Moderate/Hard/Exit present |
| `Click_TriggersFlipPiece` | Via GTK test utilities |

## Test Design Principles

### Determinism
- **Seed the RNG** — `GameTable.GenerateTable` uses `Random()` without seed
- **Fix:** Add `Random` parameter or `seed` to `GameTable` constructor
- **Test:** Same seed → same board

### Isolation
- **No GTK in unit tests** — `GameEngine` depends on `Gdk.Window` for drawing
- **Fix:** Extract `IDrawingSurface` interface, mock in tests
- **Alternative:** Test logic only, skip `DrawTable`/`DrawInfoBar`

### Speed
- Unit tests: < 10ms each
- Integration tests: < 100ms each
- No UI tests in fast suite

### Coverage Targets
| Layer | Target |
|-------|--------|
| GameTable | 100% |
| Piece | 100% |
| GameEngine (logic) | 90% |
| GameEngine (rendering) | 0% (excluded) |
| MainWindow | 0% (UI, manual) |

## Test Data

### Standard Test Values

| Concept | Value |
|---------|-------|
| Grid sizes | 4, 6, 8 |
| Piece values | 0–36 (valid assets), 37–101 (invalid) |
| Timer interval | 1000ms |
| Expected pairs | 8, 18, 32 |

### Test Builders

```csharp
// Test helper
public static class TestBoard
{
    public static GameTable Create(int size, int seed = 42)
    {
        var table = new GameTable(size);
        // Use reflection or add internal constructor for seeded Random
        return table;
    }

    public static GameEngine CreateEngine(int size, Gdk.Window tableWin = null, Gdk.Window infoWin = null)
    {
        var engine = new GameEngine(size);
        // Set drawables via reflection or test-only API
        return engine;
    }
}
```

## Running Tests

### Command Line
```bash
# Build test project
msbuild MemoryBlocks.Tests.csproj

# Run tests
dotnet test MemoryBlocks.Tests.csproj --logger "console;verbosity=detailed"

# With coverage
dotnet test MemoryBlocks.Tests.csproj --collect:"XPlat Code Coverage"
```

### CI Integration (GitHub Actions)
```yaml
# .github/workflows/dotnet.yml addition
- name: Run tests
  run: dotnet test MemoryBlocks.Tests.csproj --configuration Release
```

## Current Gaps

| Gap | Impact | Effort |
|-----|--------|--------|
| No test project | Zero verification | Medium |
| No RNG seeding | Non-deterministic tests | Low |
| GTK dependency in GameEngine | Hard to unit test | Medium |
| No CI test execution | No regression detection | Low |
| No coverage tracking | Unknown quality | Low |

## Recommended First Steps

1. **Create test project** with NUnit + Moq
2. **Add seed parameter** to `GameTable` for deterministic boards
3. **Extract drawing interface** from `GameEngine` for testability
4. **Write GameTable tests** first (pure logic, no dependencies)
5. **Write GameEngine logic tests** with mocked drawing
6. **Add to CI** — run on every push/PR
7. **Enforce coverage gate** — fail if < 80% on domain logic

## Testing the Asset/Value Mismatch

Critical test to add immediately:
```csharp
[Test]
public void GameTable_MaxValue_DoesNotExceedAssetCount()
{
    var table = new GameTable(6);
    var maxValue = table.Pieces.Cast<Piece>().Max(p => p.Value);
    Assert.That(maxValue, Is.LessThan(37), "Piece values exceed available assets (0-36)");
}
```

This would catch the `piecesMaxValue = 102` vs 37 assets bug at test time.