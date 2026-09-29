# M2 — Procedural Generation & Validation

## Implemented
- Deterministic seed-based PRNG independent of UnityEngine.Random.
- Solver-in-the-loop PuzzleGenerator.
- LevelDefinition and GenerationResult data contracts.
- Static-state + authoritative-solver LevelValidator.
- Separate DifficultyAnalyzer and InterestingnessAnalyzer.
- Replay-verified ReverseTransitionModel for future backwards generation.
- EditMode regression tests for deterministic generation and reverse transitions.

## Acceptance contract
A generated candidate is accepted only after:
1. Board static invariants pass.
2. The authoritative DeterministicSolver finds a solution.
3. Minimum solution depth is met.
4. Minimum interaction/interestingness threshold is met.

## Current generator scope
The first M2 candidate builder uses a fully connected logical road grid, two destination exits, genuine multi-cell vehicles, and optional Vector Gates. This is intentionally a correctness-first intermediate generator. It is not yet the final handcrafted-quality campaign generator.

## Verification
- Source implementation: IMPLEMENTED
- Repository diff reviewed: VERIFIED
- Unity compilation: UNVERIFIED
- Unity EditMode execution: UNVERIFIED
- Android build: BLOCKED until a real Unity Editor/Android toolchain is available

Unity's Test Framework supports EditMode and PlayMode tests, but merely authoring tests is not execution; this branch therefore makes no runtime verification claim.
