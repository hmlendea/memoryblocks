# Dependencies

This document describes external and internal dependencies.

## External Dependencies

### Runtime Dependencies

| Dependency | Version | Purpose | Integration |
|------------|---------|---------|-------------|
| Mono Runtime | 4.5+ | .NET execution environment | System runtime; required on Linux |
| .NET Framework | 4.5 | Windows execution environment | Alternative to Mono on Windows |
| GTK# (`gtk-sharp`) | 2.12 | Windowing toolkit, widgets | `Gtk.Window`, `Gtk.DrawingArea`, `Gtk.MenuBar`, etc. |
| GDK# (`gdk-sharp`) | 2.12 | Drawing surfaces, events | `Gdk.Window`, `Gdk.EventMask`, `Gdk.Pixbuf` |
| GLib# (`glib-sharp`) | 2.12 | Main loop, timeout, types | `GLib.Timeout`, `GLib.TimeoutHandler` |
| Pango# (`pango-sharp`) | 2.12 | Text layout | Used indirectly by GTK# |
| ATK# (`atk-sharp`) | 2.12 | Accessibility | Used indirectly by GTK# |
| Glade# (`glade-sharp`) | 2.12 | UI designer integration | Stetic designer support |
| `gtk-dotnet` | 2.12 | GDK ↔ System.Drawing bridge | `Gtk.DotNet.Graphics.FromDrawable()` |
| `System.Drawing` | 4.0 | GDI+ rendering API | `Graphics`, `Brush`, `Font`, `Bitmap`, `Color`, `Rectangle` |
| `Mono.Posix` | 4.0 | POSIX interop | Platform integration |

### Build Dependencies

| Dependency | Purpose |
|------------|---------|
| MonoDevelop / Visual Studio | IDE, build system |
| `xbuild` / `msbuild` | Build runners |
| `pkg-config` | GTK# library discovery (Linux) |
| GTK# 2.12 development packages | Headers, libraries for compilation |

### Packaging Dependencies

| Dependency | Purpose |
|------------|---------|
| `tar`, `gzip` | Linux package creation (`.tar.gz`) |
| `zip` | Windows package creation (`.zip`) |

## Internal Dependencies

### Project Structure

```
MemoryBlocks (exe)
├── Program.cs → MainClass
├── MainWindow.cs → MainWindow
├── Game/
│   ├── GameEngine.cs → GameEngine
│   ├── GameTable.cs → GameTable
│   └── Piece.cs → Piece
├── Utils/
│   └── Extensions.cs → MyExtensions
└── gtk-gui/ (Stetic-generated)
    ├── generated.cs
    ├── gui.stetic
    └── MemoryBlocks.MainWindow.cs
```

### Dependency Graph

```
Program.cs (MainClass)
    └─► MainWindow.cs (MainWindow)
          ├─► GameEngine.cs (GameEngine)
          │     ├─► GameTable.cs (GameTable)
          │     │     └─► Piece.cs (Piece[,])
          │     ├─► Gdk.Window (via properties)
          │     ├─► Gtk.DotNet.Graphics
          │     └─► System.Drawing
          ├─► Gtk.Window, Gtk.DrawingArea, Gtk.MenuBar, etc.
          ├─► Gdk.Window, Gdk.EventMask
          └─► MyExtensions (Utils/Extensions.cs)
```

### Namespace Dependencies

| Namespace | Depends On |
|-----------|------------|
| `MemoryBlocks` | `MemoryBlocks.Game`, `Gtk`, `Gdk` |
| `MemoryBlocks.Game` | `System`, `System.Collections.Generic`, `System.Drawing`, `System.IO`, `System.Timers`, `Gtk` (for `Gdk.Window` only) |
| `MemoryBlocks.Utils` | `System.ComponentModel`, `System.Drawing`, `Gtk`, `Gdk` |

### Coupling Analysis

#### Loose Coupling (Good)
- `GameTable` and `Piece` have **zero** external dependencies beyond BCL
- `GameEngine` depends on `Gdk.Window` only (not full GTK widget hierarchy)
- `MainWindow` depends on `GameEngine` but not on `GameTable`/`Piece` directly

#### Tight Coupling (Acceptable for Scope)
- `MainWindow` directly instantiates `GameEngine` (`new GameEngine(6)`)
- `GameEngine` directly instantiates `GameTable` (`new GameTable(tableSize)`)
- `GameTable` directly instantiates `Piece` (`new Piece()`)
- No interfaces, no DI, no factories

#### Circular Dependencies
- **None** — strict acyclic dependency graph

## Dependency Direction Rules

1. **UI → Domain** — `MainWindow` calls `GameEngine` methods
2. **Domain → Data** — `GameEngine` uses `GameTable`, `GameTable` owns `Piece[,]`
3. **Domain ↛ UI** — `GameEngine`/`GameTable`/`Piece` never reference GTK widgets
4. **Domain → Rendering** — `GameEngine` uses `System.Drawing` via `Gtk.DotNet`
5. **Utils → All** — `MyExtensions` provides helpers used by `GameEngine` and `MainWindow`

## Version Constraints

| Dependency | Constraint | Reason |
|------------|------------|--------|
| GTK# | 2.12 exactly | Stetic designer, API compatibility |
| .NET Framework | 4.5 | Mono profile, `gtk-dotnet` compatibility |
| Platform | x86 | `gtk-dotnet` 32-bit only on some distributions |
| `System.Drawing` | Mono's libgdiplus | Linux GDI+ implementation |

## Known Dependency Issues

### 1. GTK2 End-of-Life
- GTK2 no longer receives upstream security updates
- Distribution packages may be removed in future releases
- Migration to GTK3/GTK4 requires significant rewrite

### 2. `System.Drawing.Common` Platform Restriction
- In .NET Core 3.0+/ .NET 5+, `System.Drawing.Common` only supported on Windows
- This codebase targets .NET Framework 4.5 / Mono, so currently works on Linux via libgdiplus
- Migration to modern .NET would require rendering backend change (Cairo, SkiaSharp, etc.)

### 3. `gtk-dotnet` Bridge Maintenance
- `gtk-dotnet` is part of GTK# 2.x, not actively developed
- Bridge between GDK drawables and `System.Drawing.Graphics`
- Potential rendering differences vs native Windows GDI+

### 4. Asset/Value Mismatch
- `GameTable.piecesMaxValue = 102` (values 0–101)
- Only 37 piece images exist (`piece0.png`–`piece36.png`)
- Values 37–101 cause runtime resource load failure

### 5. No Dependency Management
- No NuGet packages — all references are assembly references in `.csproj`
- GTK# assemblies resolved via GAC / pkg-config on Linux
- No lock file, no version pinning beyond `.csproj` hints

## Dependency Update Process

Current process (manual):
1. Update GTK#/Mono installation on build machine
2. Verify `.csproj` reference paths still valid
3. Build solution: `xbuild MemoryBlocks.sln`
4. Test manually

No automated dependency scanning, no vulnerability scanning, no update notifications.

## Transitive Dependencies

### GTK# 2.12 Transitive Closure
```
gtk-sharp
├── gdk-sharp
│   ├── glib-sharp
│   └── pango-sharp
├── glib-sharp
├── pango-sharp
├── atk-sharp
│   └── glib-sharp
└── glade-sharp
    ├── gtk-sharp
    └── glib-sharp
```

### System.Drawing on Mono
```
System.Drawing
└── libgdiplus (native)
    ├── cairo
    ├── freetype
    ├── fontconfig
    ├── libpng
    ├── libjpeg
    ├── libtiff
    └── giflib
```

## Internal Dependency Validation

### Build Order (Enforced by Solution)
1. `MemoryBlocks` (main project)
2. `Packages` (packaging project, depends on MemoryBlocks output)

### Circular Reference Check
- Verified: No circular references in `.csproj` or `.sln`
- `MemoryBlocks` does not reference `Packages`
- `Packages` references `MemoryBlocks` output only

## Missing Dependencies (Not Used But Could Be)

| Category | Candidate | Reason Not Used |
|----------|-----------|-----------------|
| Logging | Serilog, NLog, log4net | No logging implemented |
| Testing | NUnit, xUnit, MSTest | No test project |
| DI | Microsoft.Extensions.DependencyInjection | No DI container |
| Config | Microsoft.Extensions.Configuration | No config system |
| JSON | System.Text.Json, Newtonsoft.Json | No JSON usage |
| HTTP | HttpClient, RestSharp | No network calls |