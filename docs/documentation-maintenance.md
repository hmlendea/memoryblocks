# Documentation Maintenance

This document describes how to maintain the documentation in the `docs/` directory.

## Documentation Structure

```
docs/
├── INDEX.md                           # Master index
├── architecture.md                    # System architecture
├── design-decisions.md                # Key decisions with rationale
├── repository-structure.md            # Source tree layout
├── repository-overview.md             # Purpose, scope, entry points
├── components/
│   ├── presentation.md                # MainWindow, GTK widgets
│   ├── host-and-composition.md        # Program.cs, composition root
│   ├── application-services.md        # GameEngine deep dive
│   └── integration-models.md          # GameTable, Piece
├── flows/
│   ├── startup-and-rendering.md       # Initialisation, first render
│   └── tile-flip-and-match.md         # Flip logic, match evaluation
├── behaviour/                         # (N/A for this project)
├── api-reference/                     # (N/A - no HTTP API)
├── configuration.md                   # Config schema, sources
├── dependencies.md                    # External/internal deps
├── error-handling.md                  # Error strategies, gaps
├── logging.md                         # Logging infrastructure
├── security.md                        # Threat model, mitigations
├── state-and-persistence.md           # State management
├── testing.md                         # Test strategy
├── build-and-deployment.md            # Build process, packaging
├── concurrency-and-scheduling.md      # Threading, scheduling
├── data-model.md                      # Core data structures
├── invariants.md                      # System invariants
├── integrations.md                    # External integrations
├── quick-start.md                     # Fastest path to run
├── troubleshooting.md                 # Common issues
├── faq.md                             # Frequently asked questions
├── ambiguities-and-open-questions.md  # Unresolved items
├── change-guide.md                    # How to make common changes
└── documentation-maintenance.md       # This file
```

## Maintenance Principles

### 1. Documentation as Code
- Documentation lives in the repository alongside code
- Changes to code should trigger documentation updates
- Review documentation in code reviews

### 2. Single Source of Truth
- Code is the ultimate source of truth
- Documentation reflects code, not aspirations
- If code and docs disagree, code wins — update docs

### 3. Evidence-Based
- All claims backed by file paths, line numbers, or quotes
- No speculative "should" or "will" without tracking issue
- Link to source: `MemoryBlocks/Game/GameEngine.cs:42`

### 4. Minimal Duplication
- Each fact documented in one place
- Cross-reference with links, don't copy
- `INDEX.md` links to everything; don't repeat content

## Update Triggers

### When to Update Documentation

| Trigger | Files to Check |
|---------|----------------|
| New feature added | `architecture.md`, `components/`, `flows/`, `data-model.md`, `invariants.md` |
| Bug fixed | `error-handling.md`, `troubleshooting.md`, `invariants.md` |
| Dependency added/removed | `dependencies.md`, `build-and-deployment.md` |
| Configuration changed | `configuration.md`, `quick-start.md` |
| New test added | `testing.md` |
| Security issue found | `security.md`, `error-handling.md` |
| Performance improvement | `concurrency-and-scheduling.md`, `data-model.md` |
| Breaking change | `change-guide.md`, `architecture.md`, `INDEX.md` |
| Ambiguity resolved | `ambiguities-and-open-questions.md`, `design-decisions.md` |

### When NOT to Update
- Refactoring with no behavioural change
- Whitespace/formatting only
- Comment updates in code (unless public API)

## Update Process

### 1. Identify Affected Files
```bash
# Find docs mentioning a symbol
grep -r "GameEngine" docs/
grep -r "FlipPiece" docs/
```

### 2. Make Changes
- Edit the specific file(s)
- Keep style consistent (tables, headings, code blocks)
- Update cross-references if structure changes

### 3. Verify Links
```bash
# Check for broken internal links
# (manual - no automated tool in this repo)
```

### 4. Update INDEX.md if Structure Changed
- New file → add to catalogue
- Removed file → remove from catalogue
- Moved file → update link

### 5. Commit with Descriptive Message
```
docs: update tile-flip-and-match flow for bounds check fix

- Add bounds checking section in tile-flip-and-match.md
- Update error-handling.md with new validation
- Update invariants.md I16 status to HELD
```

## Style Guide

### Headings
- `#` for document title (once per file)
- `##` for major sections
- `###` for subsections
- `####` for details

### Code Blocks
- Use language tag: ```csharp, ```bash, ```xml, ```json
- Keep lines < 100 chars
- Show context, not just changed lines

### Tables
- Use for structured data (config, dependencies, invariants)
- Header row required
- Align pipes for readability in source

### Cross-References
- Relative links: `[Architecture](architecture.md)`
- Section links: `[Invariants](invariants.md#i16-click-coordinates-in-bounds)`
- External links: full URL

### Emphasis
- **Bold** for key terms, status values
- `Code` for symbols, paths, commands
- *Italic* for first use of terms

## Quality Checks

### Before Merging
- [ ] All internal links resolve
- [ ] No TODO/FIXME left in docs (move to ambiguities)
- [ ] Code snippets compile (if complete)
- [ ] File paths exist in repository
- [ ] No stale "current status" claims

### Periodic Review (Quarterly)
- [ ] Read `ambiguities-and-open-questions.md` — resolve or update
- [ ] Check `troubleshooting.md` against recent issues
- [ ] Verify `quick-start.md` works on clean machine
- [ ] Update `dependencies.md` versions
- [ ] Review `architecture.md` for drift

## Tooling

### Current
- Manual editing in VS Code
- Markdown preview for rendering
- `grep` for cross-reference search

### Recommended Additions
- `markdownlint` for style consistency
- `lychee` for link checking
- `vale` for prose linting
- GitHub Actions for automated checks

## Versioning

- Documentation versioned with code (same git history)
- No separate version numbers
- `INDEX.md` shows "Last updated" date (manual)

## Archiving

- Obsolete docs → move to `docs/archive/` with date prefix
- Keep git history — don't delete
- Update `INDEX.md` to remove links

## Contributing

See [CONTRIBUTING.md](../CONTRIBUTING.md) for general guidelines.

### Documentation-Specific
1. Follow style guide above
2. One logical change per commit
3. Reference issue/PR in commit message
4. Update `ambiguities-and-open-questions.md` if discovering new unknowns

## Metrics (Optional)

| Metric | Target |
|--------|--------|
| Coverage (code symbols documented) | > 80% |
| Link validity | 100% |
| Stale content (< 6 months) | 0 |
| Ambiguities open | < 10 |

## Contact

Documentation maintainer: (repository owner)
Questions: GitHub Issues with `documentation` label