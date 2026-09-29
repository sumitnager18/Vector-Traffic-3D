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
