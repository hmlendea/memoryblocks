[![License](https://img.shields.io/github/license/hmlendea/memoryblocks)](https://github.com/hmlendea/memoryblocks/blob/master/LICENSE)
[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)

# Memory Blocks

Memory Blocks is a local desktop tile-matching puzzle game implemented in C# targeting Mono and GTK# 2.12. The game presents a grid of face-down tiles; players flip two tiles per turn, attempting to find matching pairs. The goal is to clear the board by matching all pairs. The application runs entirely on the user's local system with no network communication, personal data collection, or external integrations.

## 📑 Table of Contents

- [Capabilities](#-capabilities)
- [System Requirements](#-system-requirements)
- [Installation](#-installation)
  - [Installation from Source](#installation-from-source)
- [Development](#-development)
  - [Requirements](#requirements)
  - [Setup](#setup)
  - [Build](#build)
  - [Run](#run)
- [Architecture](#-architecture)
- [Usage](#-usage)
- [Roadmap](#-roadmap)
- [Documentation](#-documentation)
- [Privacy and Data](#-privacy-and-data)
- [Contributing](#-contributing)
- [Code of Conduct](#-code-of-conduct)
- [Security](#-security)
- [Project Engagement](#-project-engagement)
- [License](#-license)

## ✨ Capabilities

- Tile-matching puzzle gameplay with configurable board sizes
- Event-driven Model-View-Presenter (MVP) architecture
- Local desktop application with no network dependencies
- Embedded graphical assets (card pieces, icons)
- Timer and flip counter for scoring
- Cross-platform on Linux via Mono and GTK# 2.12

## 🖥️ System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| Operating System | Linux (any modern distribution) | Linux (any modern distribution) |
| Runtime | Mono 6.0+ | Mono 6.12+ |
| UI Framework | GTK# 2.12 | GTK# 2.12 |
| Build Tools | MonoDevelop or msbuild | MonoDevelop or msbuild |

## 📦 Installation

### Installation from Source

```bash
git clone https://github.com/hmlendea/memoryblocks.git
cd memoryblocks
msbuild MemoryBlocks.sln
```

## 🛠️ Development

### Requirements

- [Mono 6.0+ SDK](https://www.mono-project.com/download/stable/)
- [GTK# 2.12](https://www.mono-project.com/docs/gui/gtksharp/)
- MonoDevelop or msbuild

### Setup

```bash
git clone https://github.com/hmlendea/memoryblocks.git
cd memoryblocks
```

### Build

```bash
msbuild MemoryBlocks.sln
```

### Run

```bash
mono MemoryBlocks/bin/Debug/MemoryBlocks.exe
```

## 🏗️ Architecture

Memory Blocks follows an event-driven Model-View-Presenter (MVP) architecture suitable for GTK-based desktop applications.

See the [ARCHITECTURE.md](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 🚀 Usage

Run the compiled executable:

```bash
mono MemoryBlocks/bin/Debug/MemoryBlocks.exe
```

Click on tiles to flip them. Match pairs to clear the board.

## 🗺️ Roadmap

See [ROADMAP.md](./ROADMAP.md) for planned work and forthcoming changes.

## 📚 Documentation

| Resource | Description |
|----------|-------------|
| [ARCHITECTURE.md](./ARCHITECTURE.md) | High-level architecture, components, runtime flows, and design constraints |
| [SECURITY.md](./SECURITY.md) | Security policy, supported versions, vulnerability reporting |
| [PRIVACY.md](./PRIVACY.md) | Privacy and personal data handling |
| [CONTRIBUTING.md](./CONTRIBUTING.md) | Contribution guidelines and processes |
| [CODE_OF_CONDUCT.md](./CODE_OF_CONDUCT.md) | Community conduct expectations |
| [docs/INDEX.md](./docs/INDEX.md) | Complete documentation catalogue |

## 🛡️ Privacy and Data

Memory Blocks is a local desktop tile-matching puzzle game with no personal data collection, network communication, or external integrations.

For the detailed description of how the application handles privacy and personal data, see [PRIVACY.md](./PRIVACY.md).

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Preserve the existing public contract unless a breaking change is intentional
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Include tests for additional or modified functionality
- Raise a new [issue](https://github.com/hmlendea/memoryblocks/issues) for problems or suggestions

See the [contributing guidelines](./CONTRIBUTING.md) for details on how to report issues, suggest enhancements, and submit changes.

## 📜 Code of Conduct

See the [CODE_OF_CONDUCT.md](./CODE_OF_CONDUCT.md) for the standards of conduct expected in all project spaces.

## 🔒 Security

Memory Blocks takes security seriously and aims to address any reported vulnerabilities promptly through coordinated disclosure.

For information on reporting security vulnerabilities, see [SECURITY.md](./SECURITY.md).

## 💝 Project Engagement

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/memoryblocks/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License v3.0 or later`.
See [LICENSE](./LICENSE) for additional information.
