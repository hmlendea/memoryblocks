# Contributing to Memory Blocks

This document covers the guidelines and processes for contributing to this project, including how to report issues, suggest enhancements, submit code changes, and follow project standards.

## 📑 Table of Contents

- [How to Contribute](#how-to-contribute)
  - [Reporting Issues](#reporting-issues)
  - [Suggesting Enhancements](#suggesting-enhancements)
  - [Code Contributions](#code-contributions)
    - [Prerequisites](#prerequisites)
    - [Development Setup](#development-setup)
    - [Making Changes](#making-changes)
    - [Pull Request Guidelines](#pull-request-guidelines)
  - [Code Style](#code-style)
  - [Testing](#testing)
  - [Documentation](#documentation)
- [Code of Conduct](#code-of-conduct)
- [Security](#security)
- [License](#license)
- [Recognition](#recognition)
- [Getting Assistance](#getting-assistance)

## 🤝 How to Contribute

### Reporting Issues

- Search existing issues first.
- Use the issue templates if available.
- Provide clear reproduction steps.
- Include environment details.

### Suggesting Enhancements

- Check the roadmap and existing discussions.
- Explain the use case and expected behaviour.
- Consider implementation complexity.

### Code Contributions

#### Prerequisites

- .NET Framework 4.5 or compatible Mono runtime
- GTK# 2.12 or later
- MonoDevelop or Visual Studio with Mono support

#### Development Setup

```bash
# Clone the repository
git clone https://github.com/hmlendea/memoryblocks.git
cd memoryblocks

# Open the solution in MonoDevelop or Visual Studio
# Build the solution to restore dependencies
```

#### Making Changes

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Make your changes.
4. Build the solution to verify compilation.
5. Run the application to test manually.
6. Commit with clear and descriptive messages.
7. Push to your fork.
8. Open a Pull Request.

#### Pull Request Guidelines

- Target the `main` branch.
- Keep PRs focused and atomic.
- Revise documentation if applicable.
- Ensure the build passes.

### Code Style

Follow the project's coding standards:
- Use 4 spaces for indentation (no tabs)
- Follow C# naming conventions (PascalCase for public members, camelCase for private fields)
- Place opening braces on new lines
- One type per file
- XML documentation for public APIs

### Testing

No automated test suite exists. Test manually by running the application and verifying gameplay functionality.

### Documentation

- Revise relevant docs for changes.
- Update README.md if new features are added.
- Update PRIVACY.md if data handling changes.

## 📋 Code of Conduct

This project follows the [CODE_OF_CONDUCT.md](./CODE_OF_CONDUCT.md).

## 🔒 Security

Report security vulnerabilities by opening a private security advisory on GitHub or emailing the maintainers directly.

## 📄 License

By contributing, you agree that your contributions will be licensed under the GNU General Public License v3.0.

## 🙏 Recognition

Contributors are recognized in the GitHub contributors list.

## ❓ Getting Assistance

- Open a GitHub issue for questions or problems.
- Check existing discussions and documentation first.