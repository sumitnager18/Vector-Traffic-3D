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
        public Direction? TravelDirection;
        public bool IsExitAction;
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
    /// Deterministic BFS over the actual logical action space.
    /// Every vehicle movement is a one-cell transition through MoveValidator.
    /// Exit completion is a separate action only when the vehicle head is at an exit portal.
    /// Gate cycling is another explicit action.
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

            visited.Add(initialBoard.GetCanonicalStateHash());
            queue.Enqueue((initialBoard.DeepClone(), new List<SolverStep>()));

            while (queue.Count > 0 && result.ExploredStatesCount < _maxExploredStates)
            {
                var item = queue.Dequeue();
                var currentState = item.state;
                var currentSteps = item.steps;
                result.ExploredStatesCount++;

                if (currentState.IsSolved())
                {
                    result.IsSolvable = true;
                    result.Steps = currentSteps;
                    return result;
                }

                // Action 1: complete an exit only after the vehicle is physically
                // positioned at a valid exit portal.
                foreach (var vehicle in currentState.Vehicles.Values)
                {
                    if (vehicle.IsExited || !currentState.Exits.ContainsKey(vehicle.HeadPosition))
                        continue;

                    var nextState = currentState.DeepClone();
                    if (MoveValidator.TryExecuteExit(
                            nextState,
                            vehicle.Id,
                            out _,
                            out _))
                    {
                        EnqueueIfNew(
                            visited,
                            queue,
                            nextState,
                            currentSteps,
                            new SolverStep
                            {
                                VehicleId = vehicle.Id,
                                IsExitAction = true,
                                Description = $"Vehicle {vehicle.Id} completed its exit."
                            });
                    }
                }

                // Action 2: one logical vehicle movement transition.
                foreach (var vehicle in currentState.Vehicles.Values)
                {
                    if (vehicle.IsExited || currentState.Exits.ContainsKey(vehicle.HeadPosition))
                        continue;

                    foreach (var direction in MoveValidator.GetLegalStepDirections(
                                 currentState,
                                 vehicle.Id))
                    {
                        var nextState = currentState.DeepClone();

                        if (!MoveValidator.TryExecuteStep(
                                nextState,
                                vehicle.Id,
                                direction,
                                out _,
                                out _))
                            continue;

                        EnqueueIfNew(
                            visited,
                            queue,
                            nextState,
                            currentSteps,
                            new SolverStep
                            {
                                VehicleId = vehicle.Id,
                                TravelDirection = direction,
                                Description = $"Vehicle {vehicle.Id} moved one cell {direction}."
                            });
                    }
                }

                // Action 3: gate cycle.
                foreach (var gate in currentState.Gates.Values)
                {
                    var nextState = currentState.DeepClone();
                    nextState.Gates[gate.Id].CycleDirection();

                    EnqueueIfNew(
                        visited,
                        queue,
                        nextState,
                        currentSteps,
                        new SolverStep
                        {
                            VehicleId = -1,
                            ToggledGateId = gate.Id,
                            Description = $"Cycled Gate {gate.Id} to {nextState.Gates[gate.Id].AllowedDirection}."
                        });
                }
            }

            result.IsSolvable = false;
            return result;
        }

        private static void EnqueueIfNew(
            HashSet<string> visited,
            Queue<(BoardState state, List<SolverStep> steps)> queue,
            BoardState nextState,
            List<SolverStep> currentSteps,
            SolverStep step)
        {
            string hash = nextState.GetCanonicalStateHash();
            if (!visited.Add(hash))
                return;

            var nextSteps = new List<SolverStep>(currentSteps)
            {
                step
            };

            queue.Enqueue((nextState, nextSteps));
        }
    }
}
