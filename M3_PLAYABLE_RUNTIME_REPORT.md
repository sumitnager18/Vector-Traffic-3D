# M3 — First Playable 3D Runtime

## Implemented
- Grid-to-world mapper for deterministic logical-to-3D positioning.
- Runtime vehicle views with genuine footprint-scaled body geometry.
- Runtime camera and directional lighting.
- Touch/mouse selection through physics raycasts.
- Tap-to-move using the vehicle's current vector.
- Swipe-to-move using cardinal directions, allowing route turns when the logical road graph permits them.
- Every attempted movement goes through the authoritative MoveValidator.
- Logical movement is animated only after validation succeeds.
- Exit completion uses the existing authoritative CanCompleteExit / TryExecuteExit contract.
- Runtime bootstrap creates the gameplay root automatically after scene load.

## Important limitation
This is a functional runtime foundation, not final art. The generated vehicle meshes are intentionally primitive so gameplay correctness can be verified before investing in production assets.

## Verification
- Runtime source: IMPLEMENTED
- Repository diff review: VERIFIED
- Unity compilation: UNVERIFIED
- Unity PlayMode execution: UNVERIFIED
- Android device execution: UNVERIFIED
- Production art/audio/UI: NOT IMPLEMENTED

The repository currently does not contain a dedicated authored gameplay scene; the bootstrap is designed to initialize from an otherwise empty scene. A real Unity Editor run is required before calling the first playable loop TESTED.
