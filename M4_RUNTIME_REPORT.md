# M4 — Presentation, Level Loading & Runtime Input

## Scope
M4 turns the M3 runtime loop into a more coherent playable foundation without pretending it is production-ready.

### Implemented
- Unity Input System 1.17 package for the Unity 6000.0 project.
- Touch-first runtime pointer handling with mouse fallback for editor testing.
- Deterministic runtime level loading through the existing solver-validated generator.
- Safe deterministic fallback level when generation cannot produce a valid candidate.
- Programmatic road-tile presentation, exit portals, and interactive Vector Gate visuals.
- Vector Gate interaction: tap a gate to cycle its allowed direction.
- Stylized procedural vehicle presentation with body, cabin, windshield and wheels.
- Vehicle selection feedback.
- Smooth camera follow/controller and improved lighting.
- Runtime-level invariants and deterministic-loading tests.

## Important boundaries
- No claim of Unity compilation or Test Runner execution has been made.
- No Android APK/device validation has been performed.
- No authored production Unity scene has been added.
- Art remains procedural placeholder presentation; final art assets are not implemented.
- UI, audio, haptics, campaign progression, save/load and monetization are still future milestones.
- The procedural generator remains the M2 correctness-first generator; it is not yet the final reverse-construction production generator.

## Verification status
| Area | Status |
|---|---|
| GitHub source/diff | VERIFIED |
| Input System migration in source | IMPLEMENTED |
| Runtime level factory | IMPLEMENTED |
| Runtime presentation | IMPLEMENTED |
| EditMode tests authored | IMPLEMENTED / UNVERIFIED |
| Unity compilation | UNVERIFIED |
| Unity Test Runner | UNVERIFIED |
| Android build | UNVERIFIED |
| Physical Android device | UNVERIFIED |

## Red-line check
No new intentional red-line violation was introduced. In particular, gameplay movement and gate state still route through the logical model rather than visual transforms.

## Next gate
Before merging M4, a real Unity 6 editor environment must compile the project and run the EditMode/PlayMode tests. After that, M5 should focus on production gameplay UX: HUD, level-complete flow, undo, solver-powered hint presentation, accessibility controls, and authored campaign level loading.
