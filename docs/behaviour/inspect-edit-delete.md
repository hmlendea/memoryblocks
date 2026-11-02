# Inspect, Edit, and Delete Behaviour

## Applicability

**Status: Not Applicable (N/A)**

`MemoryBlocks` is a desktop tile-matching puzzle game built using GTK#. It does not implement entity inspection, editing, or deletion workflows.

## Domain Overview

The game operates on a fixed $10 \times 10$ board of memory tiles. Tiles can only be flipped (revealed) or cleared (matched). There are no editable records, deletable entities, or inspection views within the application.

## Tile State Transitions

The only state mutations available to the user are:
- **Flip**: Reveal a tile's face image temporarily.
- **Unflip**: Hide a tile's face image after a mismatch or timeout.
- **Clear**: Permanently remove a matched pair from the board.

These transitions are documented in [Tile Flip and Match Flow](../flows/tile-flip-and-match.md).

## Potential Future Expansion

If inspection, editing, or deletion capabilities are added in future versions (e.g., custom level editing, tile management, or profile deletion), their design decisions and behavioural flows will be documented here.

## References

- [Architecture Overview](../architecture.md)
- [Presentation Component](../components/presentation.md)
- [State and Persistence](../state-and-persistence.md)
