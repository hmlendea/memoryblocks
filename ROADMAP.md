# Memory Blocks Roadmap

This roadmap outlines the planned work and forthcoming changes for Memory Blocks, a local desktop tile-matching puzzle game implemented in C# targeting Mono and GTK# 2.12.

## 📑 Table of Contents

- [Current Focus](#-current-focus)
- [Planned Labour](#-planned-labour)
- [Out of Scope](#-out-of-scope)
- [Contribution](#-contribution)
- [Support](#-support)

## 🎯 Current Focus

- **Status: In Progress**: Modernise build system to .NET SDK-style projects — Migrate from legacy .csproj to SDK-style format with proper target framework
- **Status: Planned**: Add unit tests for core game logic — Implement tests for GameEngine, GameTable, and Piece classes

## 🗺️ Planned Labour

- Migrate to .NET SDK-style project format
- Add an architecture tests project with NUnit
- Add a unit tests project with NUnit
- Implement CI/CD pipeline with GitHub Actions
- Add configuration file support for board sizes
- Implement high score persistence
- Add keyboard navigation support
- Improve accessibility (screen reader support)

## 🚫 Out of Scope

- Mobile platform ports
- Web-based version

## 🤝 Contribution

External contributions are welcome. Propose roadmap refinements or implementation suggestions via repository [issues](https://github.com/hmlendea/memoryblocks/issues) and focused pull requests aligned with current priorities.

## 🆘 Support

For roadmap questions, proposals, or clarifications, [open an issue](https://github.com/hmlendea/memoryblocks/issues).