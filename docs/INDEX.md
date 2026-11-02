# Memory Blocks Documentation

## Repository Summary

Memory Blocks is a local desktop tile-matching puzzle game implemented in C# targeting Mono and GTK# 2.12. The game presents a grid of face-down tiles; players flip two tiles per turn, attempting to find matching pairs. The goal is to clear the board by matching all pairs. The application runs entirely on the user's local system with no network communication, personal data collection, or external integrations.

## Root Document Map

- [ARCHITECTURE.md](../ARCHITECTURE.md) — High-level architecture, components, runtime flows, and design constraints
- [SECURITY.md](../SECURITY.md) — Security policy, supported versions, vulnerability reporting
- [PRIVACY.md](../PRIVACY.md) — Privacy and personal data handling
- [LICENSE](../LICENSE) — GNU General Public License v3
- [CODE_OF_CONDUCT.md](../CODE_OF_CONDUCT.md) — Community conduct expectations
- [CONTRIBUTING.md](../CONTRIBUTING.md) — Contribution guidelines and processes
- [README.md](../README.md) — Project overview and quick start

## Documentation Catalogue

### Architecture & Design
- [architecture.md](./architecture.md) — Detailed architectural elaboration complementing root ARCHITECTURE.md
- [design-decisions.md](./design-decisions.md) — Key architectural and design choices with rationale
- [repository-structure.md](./repository-structure.md) — Source tree layout and module organisation
- [repository-overview.md](./repository-overview.md) — Repository purpose, scope, and entry points

### Components
- [components/presentation.md](./components/presentation.md) — UI layer: MainWindow, GTK widgets, event binding
- [components/host-and-composition.md](./components/host-and-composition.md) — Application entry point and composition root
- [components/application-services.md](./components/application-services.md) — GameEngine: session state, timer, rules, rendering
- [components/integration-models.md](./components/integration-models.md) — Domain models: GameTable, Piece

### Flows
- [flows/startup-and-rendering.md](./flows/startup-and-rendering.md) — Application initialisation and first render
- [flows/tile-flip-and-match.md](./flows/tile-flip-and-match.md) — Tile flip, match evaluation, and board update

### Behaviour
- [behaviour/browse-and-search.md](./behaviour/browse-and-search.md) — Not applicable (no browse/search in this game)
- [behaviour/inspect-edit-delete.md](./behaviour/inspect-edit-delete.md) — Not applicable (no CRUD in this game)

### Cross-Cutting
- [configuration.md](./configuration.md) — Configuration schema, sources, precedence
- [dependencies.md](./dependencies.md) — External and internal dependencies
- [error-handling.md](./error-handling.md) — Error taxonomy, handling patterns, recovery
- [logging.md](./logging.md) — Logging framework, levels, structured fields, correlation, sinks
- [security.md](./security.md) — Security model, threats, mitigations (complements root SECURITY.md)
- [state-and-persistence.md](./state-and-persistence.md) — State management, stores, caches, migrations
- [testing.md](./testing.md) — Test strategy, organisation, coverage
- [build-and-deployment.md](./build-and-deployment.md) — Build pipeline, deployment, environments
- [concurrency-and-scheduling.md](./concurrency-and-scheduling.md) — Threading, async, schedulers, locks
- [data-model.md](./data-model.md) — Domain entities, relationships, persistence
- [invariants.md](./invariants.md) — System-wide invariants and contracts
- [integrations.md](./integrations.md) — External system integrations
- [quick-start.md](./quick-start.md) — Getting started guide for newcomers
- [troubleshooting.md](./troubleshooting.md) — Common issues and solutions
- [faq.md](./faq.md) — Frequently asked questions
- [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md) — Unresolved items, TODOs, known gaps
- [change-guide.md](./change-guide.md) — How to modify common areas safely
- [documentation-maintenance.md](./documentation-maintenance.md) — How to keep docs current, ownership

### API Reference
- [api-reference/INDEX.md](./api-reference/INDEX.md) — API reference index (not applicable — no HTTP API)

## Navigation Aids

- **Start here** (newcomers): [quick-start.md](./quick-start.md) → [repository-overview.md](./repository-overview.md) → [architecture.md](./architecture.md)
- **Deep dive** (component owners): [components/](./components/) → [flows/](./flows/) → [data-model.md](./data-model.md)
- **Flows** (debuggers): [flows/startup-and-rendering.md](./flows/startup-and-rendering.md) → [flows/tile-flip-and-match.md](./flows/tile-flip-and-match.md)

## Maintenance Metadata

- Last reviewed: 2026-11-02
- Owner: Repository maintainers
- Coverage status: Initial generation — comprehensive coverage of all identified components, flows, and cross-cutting concerns