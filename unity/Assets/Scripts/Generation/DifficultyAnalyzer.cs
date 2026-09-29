using System;
using VectorTraffic3D.Board;
using VectorTraffic3D.Solver;
using VectorTraffic3D.Puzzle;
namespace VectorTraffic3D.Generation
{
    public sealed class DifficultyAnalyzer
    {
        public float Analyze(BoardState board, SolverResult solution)
        {
            if (board == null || solution == null || !solution.IsSolvable) return 100f;
            float depth = Normalize(solution.SolutionDepth, 4f, 80f);
            float branching = EstimateBranching(board);
            float dependency = EstimateDependency(solution);
            float congestion = EstimateCongestion(board);
            float footprint = EstimateFootprint(board);
            float gates = Normalize(board.Gates.Count, 0f, 3f);
            float exploration = Normalize(solution.ExploredStatesCount, 1f, 50000f);
            return Clamp01(depth * .20f + branching * .15f + dependency * .15f + congestion * .10f +
                           footprint * .10f + gates * .10f + exploration * .20f) * 100f;
        }
        private static float EstimateBranching(BoardState board)
        {
            int legal = 0, opportunities = 0;
            foreach (var v in board.Vehicles.Values)
            {
                if (v.IsExited) continue;
                legal += MoveValidator.GetLegalStepDirections(board, v.Id).Count;
                opportunities++;
            }
            return opportunities == 0 ? 1f : Normalize(legal, 0f, opportunities * 3f);
        }
        private static float EstimateDependency(SolverResult solution)
        {
            if (solution.Steps.Count <= 1) return 0f;
            int switches = 0, gates = 0, previous = -1;
            foreach (var step in solution.Steps)
            {
                if (step.ToggledGateId.HasValue) gates++;
                if (step.VehicleId >= 0 && previous >= 0 && previous != step.VehicleId) switches++;
                if (step.VehicleId >= 0) previous = step.VehicleId;
            }
            float s = Normalize(switches, 0f, solution.Steps.Count);
            float g = Normalize(gates, 0f, Math.Max(1, solution.Steps.Count / 3f));
            return Clamp01(s * .7f + g * .3f);
        }
        private static float EstimateCongestion(BoardState board)
        {
            int occupied = 0;
            foreach (var v in board.Vehicles.Values) if (!v.IsExited) occupied += v.Length;
            return board.Width * board.Height == 0 ? 1f : Clamp01(occupied / (float)(board.Width * board.Height) * 1.8f);
        }
        private static float EstimateFootprint(BoardState board)
        {
            if (board.Vehicles.Count == 0) return 0f;
            int total = 0;
            foreach (var v in board.Vehicles.Values) total += v.Length;
            return Normalize(total / (float)board.Vehicles.Count, 2f, 5f);
        }
        private static float Normalize(float value, float min, float max)
        {
            if (max <= min) return value >= max ? 1f : 0f;
            return Clamp01((value - min) / (max - min));
        }
        private static float Clamp01(float value) => Math.Max(0f, Math.Min(1f, value));
    }
}
