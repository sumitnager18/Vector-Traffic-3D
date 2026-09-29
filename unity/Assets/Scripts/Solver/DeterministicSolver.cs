using System.Collections.Generic;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Puzzle;

namespace VectorTraffic3D.Solver
{
    public struct SolverStep
    {
        public int VehicleId;
        public int? ToggledGateId;
        public string Description;
    }

    public class SolverResult
    {
        public bool IsSolvable { get; set; }
        public List<SolverStep> Steps { get; set; } = new List<SolverStep>();
        public int ExploredStatesCount { get; set; }
        public int SolutionDepth => Steps.Count;
    }

    /// <summary>
    /// Deterministic state-space solver.
    /// Strictly uses the EXACT authoritative MoveValidator rules as player-facing gameplay.
    /// Evaluates complete multi-cell footprints, exit clearance, and gate cycles.
    /// </summary>
    public class DeterministicSolver
    {
        private readonly int _maxExploredStates;

        public DeterministicSolver(int maxExploredStates = 50000)
        {
            _maxExploredStates = maxExploredStates;
        }

        public SolverResult Solve(BoardState initialBoard)
        {
            var result = new SolverResult();
            var queue = new Queue<(BoardState state, List<SolverStep> steps)>();
            var visited = new HashSet<string>();

            string initialHash = initialBoard.GetCanonicalStateHash();
            visited.Add(initialHash);
            queue.Enqueue((initialBoard.DeepClone(), new List<SolverStep>()));

            while (queue.Count > 0 && result.ExploredStatesCount < _maxExploredStates)
            {
                var (currentState, currentSteps) = queue.Dequeue();
                result.ExploredStatesCount++;

                if (currentState.IsSolved())
                {
                    result.IsSolvable = true;
                    result.Steps = currentSteps;
                    return result;
                }

                // Branch 1: Try executing exit for each active vehicle using authoritative MoveValidator
                foreach (var vehicle in currentState.Vehicles.Values)
                {
                    if (vehicle.IsExited) continue;

                    var nextState = currentState.DeepClone();
                    if (MoveValidator.TryExecuteExit(nextState, vehicle.Id, out _, out _))
                    {
                        string stateHash = nextState.GetCanonicalStateHash();
                        if (!visited.Contains(stateHash))
                        {
                            visited.Add(stateHash);
                            var nextSteps = new List<SolverStep>(currentSteps)
                            {
                                new SolverStep
                                {
                                    VehicleId = vehicle.Id,
                                    Description = $"Vehicle {vehicle.Id} ({vehicle.Type}, length {vehicle.Length}) exited via complete footprint path."
                                }
                            };
                            queue.Enqueue((nextState, nextSteps));
                        }
                    }
                }

                // Branch 2: Try cycling gates if any exist
                foreach (var gate in currentState.Gates.Values)
                {
                    var nextState = currentState.DeepClone();
                    nextState.Gates[gate.Id].CycleDirection();

                    string stateHash = nextState.GetCanonicalStateHash();
                    if (!visited.Contains(stateHash))
                    {
                        visited.Add(stateHash);
                        var nextSteps = new List<SolverStep>(currentSteps)
                        {
                            new SolverStep
                            {
                                VehicleId = -1,
                                ToggledGateId = gate.Id,
                                Description = $"Cycled Gate {gate.Id} to {nextState.Gates[gate.Id].AllowedDirection}."
                            }
                        };
                        queue.Enqueue((nextState, nextSteps));
                    }
                }
            }

            result.IsSolvable = false;
            return result;
        }
    }
}
