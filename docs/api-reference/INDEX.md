# API Reference Index

## Applicability

**Status: Not Applicable (N/A)**

`MemoryBlocks` is a desktop GTK# application. It does not expose an HTTP API, REST endpoints, GraphQL schema, gRPC services, or any network-accessible interface.

## Internal C# API

The project contains internal C# classes and methods. These are documented in the component and flow documents:

| Component | Document |
|-----------|----------|
| `Program` | [Host and Composition](../components/host-and-composition.md) |
| `MainWindow` | [Presentation](../components/presentation.md) |
| `GameEngine` | [Application Services](../components/application-services.md) |
| `GameTable` | [Integration Models](../components/integration-models.md) |
| `Piece` | [Integration Models](../components/integration-models.md) |
| `Extensions` | [Integration Models](../components/integration-models.md) |

## Potential Future Expansion

If a network API is added in future versions (e.g., online leaderboards, multiplayer, or remote configuration), this directory will contain:
- `openapi.yaml` — OpenAPI 3.1 specification
- `endpoints/` — Per-endpoint documentation
- `schemas/` — Request/response schemas
- `authentication.md` — Auth mechanisms
- `rate-limiting.md` — Quota and throttling policies

## References

- [Architecture Overview](../architecture.md)
- [Components Index](../components/)
- [Flows Index](../flows/)