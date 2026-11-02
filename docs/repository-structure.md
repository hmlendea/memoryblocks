# Repository Structure

This document describes the source tree layout and module organisation.

## Root Layout

```
memoryblocks/
├── ARCHITECTURE.md           # High-level architecture (root)
├── CODE_OF_CONDUCT.md        # Community conduct
├── CONTRIBUTING.md           # Contribution guidelines
├── LICENSE                   # GPL-3.0 license text
├── MemoryBlocks.sln          # Visual Studio solution (legacy .sln format)
├── Packages.mdproj           # MonoDevelop packaging project
├── PRIVACY.md                # Privacy policy
├── README.md                 # Project overview
├── SECURITY.md               # Security policy
├── MemoryBlocks/             # Main application project
│   ├── MainWindow.cs         # Main window, UI event handling
│   ├── Program.cs            # Application entry point
│   ├── MemoryBlocks.csproj   # Project file (.NET Framework 4.5)
│   ├── Game/                 # Game domain logic
│   │   ├── GameEngine.cs     # Session state, rules, rendering
│   │   ├── GameTable.cs      # Board data structure, pair generation
│   │   └── Piece.cs          # Tile data model
│   ├── gtk-gui/              # Stetic-generated GUI code
│   │   ├── generated.cs      # Auto-generated widget declarations
│   │   ├── gui.stetic        # Stetic GUI designer file
│   │   └── MemoryBlocks.MainWindow.cs  # Partial MainWindow (Stetic)
│   ├── Properties/
│   │   └── AssemblyInfo.cs   # Assembly metadata
│   ├── Resources/
│   │   ├── rename.sh         # Asset renaming script
│   │   └── Pieces/           # 37 PNG piece images (piece0.png–piece36.png)
│   └── Utils/
│       └── Extensions.cs     # Extension methods (Pixbuf→Bitmap, Gdk.Window helpers)
└── docs/                     # Generated documentation (this directory)
```

## Module Organisation

### MemoryBlocks (Main Project)

**Namespace:** `MemoryBlocks` (root), `MemoryBlocks.Game` (domain)

**Target Framework:** .NET Framework 4.5 (Mono profile)

**Output Type:** WinExe (GUI application)

**Platform Target:** x86 (32-bit)

**Key References:**
- `gtk-sharp`, `gdk-sharp`, `glib-sharp`, `pango-sharp`, `atk-sharp`, `glade-sharp` (GTK# 2.12)
- `gtk-dotnet` (GDK ↔ System.Drawing bridge)
- `System.Drawing` (GDI+ rendering)
- `Mono.Posix` (POSIX interop)

### Game Domain (`MemoryBlocks.Game`)

| File | Type | Responsibility |
|------|------|----------------|
| `GameEngine.cs` | Class | Session orchestration, timer, rules, rendering |
| `GameTable.cs` | Class | Board matrix, random pair generation |
| `Piece.cs` | Class | Individual tile data model |

**Dependencies:** Only `System`, `System.Collections.Generic`, `System.Drawing`, `System.IO`, `System.Timers`, `Gtk` (for `Gdk.Window` reference only)

### UI Layer (`MemoryBlocks` root namespace)

| File | Type | Responsibility |
|------|------|----------------|
| `Program.cs` | Class (`MainClass`) | Entry point, GTK init, main loop |
| `MainWindow.cs` | Partial class (`MainWindow`) | Window, menus, event binding, coordinate translation |
| `gtk-gui/generated.cs` | Auto-generated | Widget field declarations |
| `gtk-gui/MemoryBlocks.MainWindow.cs` | Partial class | Stetic `Build()` method |

### Utilities (`MemoryBlocks.Utils`)

| File | Type | Responsibility |
|------|------|----------------|
| `Extensions.cs` | Static class | `Pixbuf.ToBitmap()`, `Gdk.Window.GetWidth/Height()`, `ShowMessageDialog()` |

### Resources

- **Embedded Resources:** 37 PNG images (`piece0.png`–`piece36.png`) + `gui.stetic` + `memoryblocks.ico`
- **Resource Namespace:** `MemoryBlocks.Resources.Pieces.piece{ID}.png`
- **Loading:** `Gdk.Pixbuf.LoadFromResource()` at render time in `GameEngine.DrawTable()`

## Build Configuration

### MemoryBlocks.csproj

- **Configurations:** Debug|x86, Release|x86
- **Output Path:** `bin\Debug\` / `bin\Release\`
- **Define Constants:** `DEBUG` (Debug only)
- **Warning Level:** 4
- **Platform Target:** x86

### Solution (MemoryBlocks.sln)

- **Projects:** MemoryBlocks (main), Packages (packaging)
- **Configurations:** Debug|x86, Release|x86

### Packaging (Packages.mdproj)

- **Linux Binaries:** `MemoryBlocks-Linux-x86.tar.gz` (Release|x86)
- **Windows Binaries:** `MemoryBlocks-Windows-x86.zip` (Debug|x86)

## File Naming Conventions

- **C# Files:** PascalCase, one type per file (`GameEngine.cs`, `Piece.cs`)
- **Partial Classes:** Same name with context suffix (`MainWindow.cs`, `MemoryBlocks.MainWindow.cs`)
- **Auto-generated:** `generated.cs` (Stetic output)
- **Resources:** Lowercase with numeric suffix (`piece0.png`, `piece1.png`)
- **Scripts:** Lowercase with `.sh` extension (`rename.sh`)

## Directory Conventions

- **Game Domain:** `Game/` subdirectory under project
- **GUI Designer:** `gtk-gui/` (Stetic convention)
- **Resources:** `Resources/` with `Pieces/` subdirectory
- **Utilities:** `Utils/` for cross-cutting helpers
- **Properties:** `Properties/` for AssemblyInfo (standard .NET)