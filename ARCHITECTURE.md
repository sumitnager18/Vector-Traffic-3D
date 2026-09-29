# Vector Traffic 3D: Architecture Specification

## Layer Separation

```
+-----------------------------------------------------------+
|                      LOGICAL WORLD                        |
|                                                           |
|  - BoardState: Deterministic grid representation           |
|  - VehicleFootprint: Multi-cell occupancy calculator      |
|  - VehicleState: Vehicle attributes, vector, orientation   |
|  - OccupancySystem: Collision & overlap prevention matrix  |
|  - RouteGraph: Intersections, roads, exit nodes           |
|  - VectorGate: State-driven directional flow controllers  |
|  - MoveValidator: Exact route legality inspection          |
|  - DeterministicSolver: Canonical state search (BFS/A*)   |
|  - PuzzleGenerator: Reverse-transition synthesis engine  |
+-----------------------------------------------------------+
                             |
                             v
+-----------------------------------------------------------+
|                      PRESENTATION                         |
|                                                           |
|  - VehicleVisual: Stylized miniature 3D models & rigs     |
|  - Environment: Diorama stages, isometric projection       |
|  - Camera: Orthographic 3/4 isometric with frame-fit      |
|  - VFX / Audio / Haptics / UI                             |
+-----------------------------------------------------------+
```

The presentation layer is strictly read-only with respect to game state and move legality.
Moves are validated and committed in the logical world prior to visual animation.


## Milestone 1 — Logical Road Network

The authoritative routing layer is implemented under Assets/Scripts/Routing/. RouteGraph owns logical nodes and directed segments; RouteResolver exposes legal head transitions. BoardState owns the static route topology while its canonical dynamic hash remains focused on state-changing variables. MoveValidator is the single movement contract used by gameplay and solver. The solver now enumerates explicit one-cell intermediate movement actions before exit completion. Static topology is intentionally excluded from the dynamic hash because it does not mutate during a puzzle.
