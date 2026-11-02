# Build and Deployment

This document describes the build process, artifacts, and deployment options.

## Build System

### Solution Structure
```
MemoryBlocks.sln
├── MemoryBlocks/MemoryBlocks.csproj  → Main executable
└── Packages/Packages.mdproj          → Packaging project
```

### Build Commands

#### Linux (Mono/xbuild)
```bash
# Restore (not needed - no NuGet)
# Build Debug
xbuild MemoryBlocks.sln /p:Configuration=Debug

# Build Release
xbuild MemoryBlocks.sln /p:Configuration=Release

# Clean
xbuild MemoryBlocks.sln /t:Clean
```

#### Windows (MSBuild)
```bash
# Build Debug
msbuild MemoryBlocks.sln /p:Configuration=Debug

# Build Release
msbuild MemoryBlocks.sln /p:Configuration=Release

# Clean
msbuild MemoryBlocks.sln /t:Clean
```

#### .NET CLI (if migrated to SDK-style)
```bash
dotnet build MemoryBlocks.sln -c Release
dotnet publish MemoryBlocks/MemoryBlocks.csproj -c Release -r linux-x64 --self-contained
```

## Project Configuration (MemoryBlocks.csproj)

### Key Properties
```xml
<Project>
  <PropertyGroup>
    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>
    <OutputType>WinExe</OutputType>
    <PlatformTarget>x86</PlatformTarget>
    <AssemblyName>MemoryBlocks</AssemblyName>
    <RootNamespace>MemoryBlocks</RootNamespace>
  </PropertyGroup>
</Project>
```

### References
```xml
<ItemGroup>
  <Reference Include="gtk-sharp, Version=2.12.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f" />
  <Reference Include="gdk-sharp, Version=2.12.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f" />
  <Reference Include="glib-sharp, Version=2.12.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f" />
  <Reference Include="pango-sharp, Version=2.12.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f" />
  <Reference Include="atk-sharp, Version=2.12.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f" />
  <Reference Include="glade-sharp, Version=2.12.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f" />
  <Reference Include="gtk-dotnet, Version=2.12.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f" />
  <Reference Include="System.Drawing" />
  <Reference Include="Mono.Posix" />
</ItemGroup>
```

### Embedded Resources
```xml
<ItemGroup>
  <EmbeddedResource Include="Resources\Pieces\piece*.png" />
  <EmbeddedResource Include="gtk-gui\gui.stetic" />
  <EmbeddedResource Include="icon.png" />
</ItemGroup>
```

## Build Output

### Debug
```
MemoryBlocks/bin/Debug/
├── MemoryBlocks.exe          (managed assembly)
├── MemoryBlocks.exe.mdb      (debug symbols)
├── gtk-sharp.dll             (copied local)
├── gdk-sharp.dll
├── glib-sharp.dll
├── pango-sharp.dll
├── atk-sharp.dll
├── glade-sharp.dll
├── gtk-dotnet.dll
└── Mono.Posix.dll
```

### Release
```
MemoryBlocks/bin/Release/
├── MemoryBlocks.exe
├── (same native dependencies)
```

## Runtime Requirements

### Linux
- Mono Runtime 4.5+
- GTK# 2.12 (gtk-sharp2, libgtk2.0-0, libcairo2, libgdiplus)
- X11 or Wayland display server

### Windows
- .NET Framework 4.5+
- GTK# for .NET 2.12 installer

### macOS
- Mono 4.5+
- GTK# 2.12 via Homebrew/MacPorts
- XQuartz for X11

## Packaging (Packages.mdproj)

The `Packages` project creates distribution archives.

### Output Formats
| Format | Target | Command |
|--------|--------|---------|
| `.tar.gz` | Linux | `tar -czf MemoryBlocks-linux.tar.gz -C bin/Release .` |
| `.zip` | Windows | `zip -r MemoryBlocks-windows.zip bin/Release/` |

### Package Contents
```
MemoryBlocks-linux.tar.gz
├── MemoryBlocks.exe
├── *.dll (all GTK# dependencies)
├── icon.png
└── (no install script - run directly with mono)
```

### Installation (Linux)
```bash
tar -xzf MemoryBlocks-linux.tar.gz
cd MemoryBlocks
mono MemoryBlocks.exe
```

### Installation (Windows)
```cmd
unzip MemoryBlocks-windows.zip
MemoryBlocks.exe
```

## CI/CD Pipeline

### Current State
- No CI/CD configured
- No GitHub Actions workflows
- Manual build and package only

### Recommended Pipeline (.github/workflows/dotnet.yml)
```yaml
name: .NET Build

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Mono
        uses: actions/setup-mono@v1
        with:
          mono-version: '6.12'

      - name: Install GTK# dependencies
        run: |
          sudo apt-get update
          sudo apt-get install -y gtk-sharp2 libgtk2.0-dev libcairo2-dev libgdiplus

      - name: Build
        run: xbuild MemoryBlocks.sln /p:Configuration=Release

      - name: Test
        run: |
          # Add when test project exists
          # xbuild MemoryBlocks.Tests.csproj /p:Configuration=Release
          # mono packages/NUnit.ConsoleRunner/tools/nunit3-console.exe MemoryBlocks.Tests/bin/Release/MemoryBlocks.Tests.dll

      - name: Package Linux
        run: |
          tar -czf MemoryBlocks-linux-${{ github.sha }}.tar.gz -C MemoryBlocks/bin/Release .

      - name: Upload Artifacts
        uses: actions/upload-artifact@v4
        with:
          name: MemoryBlocks-linux
          path: MemoryBlocks-linux-*.tar.gz
```

## Deployment Strategies

### Direct Distribution
- Upload `.tar.gz`/`.zip` to GitHub Releases
- Users download and run manually
- No auto-update

### Linux Package Managers (Future)
| Format | Tool | Spec |
|--------|------|------|
| `.deb` | `dpkg-deb` | Debian/Ubuntu |
| `.rpm` | `rpmbuild` | Fedora/RHEL |
| `.apk` | `abuild` | Alpine |
| Flatpak | `flatpak-builder` | Universal |
| Snap | `snapcraft` | Universal |

### Windows Installer (Future)
- WiX Toolset → `.msi`
- Inno Setup → `.exe`
- Chocolatey package

### macOS Distribution (Future)
- `.dmg` with app bundle
- Homebrew formula
- Mac App Store (requires sandboxing, signing)

## Versioning

### Current
- No version in AssemblyInfo.cs (uses default 1.0.0.0)
- No semantic versioning
- No build metadata

### Recommended
```csharp
// Properties/AssemblyInfo.cs
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+git.{shortSha}")]
```

### Version Sources
1. `git describe --tags` for release versions
2. `git rev-parse --short HEAD` for commit hash
3. CI build number for pre-releases

## Build Verification

### Smoke Test
```bash
# Linux
mono MemoryBlocks/bin/Release/MemoryBlocks.exe &
sleep 2
kill %1
echo "Launch successful"
```

### Dependency Check
```bash
# Verify all GTK# assemblies present
ls MemoryBlocks/bin/Release/*.dll | grep -E "gtk|gdk|glib|pango|atk|glade|gtk-dotnet"
```

### Resource Check
```bash
# Verify embedded resources
monodis --resources MemoryBlocks/bin/Release/MemoryBlocks.exe | grep -E "Pieces|gui.stetic|icon"
```

## Known Build Issues

| Issue | Workaround |
|-------|------------|
| x86 required on Linux | `PlatformTarget=x86` in csproj; some distros need `mono --arch=32` |
| GTK# reference paths | Use `pkg-config --libs gtk-sharp-2.0` to verify |
| libgdiplus missing | `apt-get install libgdiplus` / `brew install mono-libgdiplus` |
| Stetic designer | Only works in MonoDevelop/Visual Studio for Mac; not VS Code |

## Migration to Modern Build

### Target: .NET 8 + GTK 4 + SDK-style csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <OutputType>WinExe</OutputType>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="GtkSharp" Version="3.24.24.*" />
    <PackageReference Include="SkiaSharp" Version="2.88.*" />
  </ItemGroup>
</Project>
```

### Breaking Changes Required
1. GTK 2 → GTK 4 API migration
2. `System.Drawing` → SkiaSharp/Cairo
3. Stetic → manual UI code or Glade XML
4. `GLib.Timeout` → `GLib.Timeout.Add` (similar)
5. Embedded resources → content files or keep embedded

## Build Performance

| Metric | Current | Target |
|--------|---------|--------|
| Clean build | ~15s | < 10s |
| Incremental build | ~3s | < 2s |
| Package creation | ~5s | < 3s |
| Binary size (Release) | ~200 KB + deps | < 500 KB total |

## Artifact Retention

| Artifact | Retain | Storage |
|----------|--------|---------|
| Release binaries | Per release | GitHub Releases |
| Debug symbols | 30 days | CI artifacts |
| Coverage reports | 30 days | CI artifacts |
| Build logs | 7 days | CI logs |