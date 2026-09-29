# Milestone 1 Report — Road Network Foundation

Project: VECTOR TRAFFIC 3D
Branch: m1-road-network-foundation
Status: IMPLEMENTED / UNVERIFIED RUNTIME

## Scope completed

M1 implemented the logical road-network foundation and upgraded the solver to operate on explicit intermediate movement states.

### Implemented

- RouteGraph: deterministic grid-backed road topology, road nodes, directed segments, bidirectional segment helper, node lookup, outgoing direction queries, junction detection, transition validation, topology hash, deep clone.
- RoadNode: stable ID, grid position, Normal/Junction/Exit type.
- RoadSegment: stable ID, source node, destination node, derived travel direction.
- Junction: incoming/outgoing segment identity with deterministic ordering.
- RouteResolver: graph-authoritative legal travel directions, legacy open-grid fallback, next-position validation.
- BoardState: owns route graph, deep-clones route topology, synchronizes exits with route nodes, keeps dynamic hashing separate from static topology.
- MoveValidator: one-cell movement transition, route-aware direction selection, full footprint validation, rotated-footprint validation, road membership, gate, vehicle, obstacle and boundary validation, explicit exit completion.
- DeterministicSolver: explicit intermediate movement states, one-cell movement actions, explicit exit action, gate-cycle actions, shared MoveValidator rules.

## Solver state model

The solver explores vehicle one-cell movement, route turns, gate state changes and exit completion. Static road topology is not included in the dynamic canonical hash because it does not change during a puzzle. Dynamic hash includes current vehicle geometry/state and gate state.

## Exit semantics

A vehicle reaching an exit portal is not automatically considered exited. The logical sequence is: movement -> head reaches exit -> explicit exit completion -> vehicle IsExited.

## Multi-cell turning

Turning is a logical transition in which the vehicle advances into a selected outgoing road direction and its footprint is recalculated using the new travel orientation. The complete rotated footprint must remain inside the board, avoid obstacles and vehicles, satisfy gates, and belong to the logical road network when a graph is active.

## Compatibility

Boards without a RouteGraph retain the original open-grid behavior: a vehicle may only advance along its current vector.

## Not implemented in M1

- procedural reverse-transition generator
- difficulty analyzer
- interestingness analyzer
- campaign levels
- 3D vehicle presentation
- 3D road rendering
- camera system
- audio/haptics
- final UI
- monetization

## Verification status

The Unity Test Framework supports EditMode and PlayMode tests. Current repository changes are AUTHORED but Unity Editor execution is UNVERIFIED in this environment. No APK has been claimed as built.

## Android configuration

The project target API was raised from 34 to 36. Unity 6 exposes Android API 36 as a target level, and Google Play requires new apps and updates submitted from August 31, 2026 to target Android 16 / API 36 or higher.

The project remains on Unity 6000.0.32f1. Unity's supported dependency documentation for the 6000.0 line lists OpenJDK 17 and NDK r23b for the relevant releases. Actual Android build execution remains blocked until a properly configured Unity workstation is used.

## M1 readiness

M1 logical implementation: IMPLEMENTED
Unity compilation: UNVERIFIED
Unity Test Runner: UNVERIFIED
Android build: BLOCKED until a real Unity Android environment is available
Procedural generator: NOT IMPLEMENTED
Ready for M2: CONDITIONAL — after Unity compilation and test execution confirm the new routing code.