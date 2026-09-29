using System;
using VectorTraffic3D.Board;
using VectorTraffic3D.Solver;
namespace VectorTraffic3D.Generation
{
    public sealed class InterestingnessAnalyzer
    {
        public float Analyze(BoardState board, SolverResult solution)
        {
            if (board == null || solution == null || !solution.IsSolvable || solution.Steps.Count == 0) return 0f;
            int switches = 0, gateActions = 0, previous = -1;
            var seen = new System.Collections.Generic.HashSet<int>();
            foreach (var step in solution.Steps)
            {
                if (step.ToggledGateId.HasValue) gateActions++;
                if (step.VehicleId >= 0)
                {
                    seen.Add(step.VehicleId);
                    if (previous >= 0 && previous != step.VehicleId) switches++;
                    previous = step.VehicleId;
                }
            }
            float interaction = Clamp01(switches / (float)Math.Max(1, solution.Steps.Count - 1));
            float gate = Clamp01(gateActions / (float)Math.Max(1, solution.Steps.Count / 3));
            float mix = Clamp01((seen.Count - 1) / 3f);
            float depth = Clamp01((solution.SolutionDepth - 4) / 30f);
            return Clamp01(interaction * .40f + gate * .25f + mix * .20f + depth * .15f) * 100f;
        }
        private static float Clamp01(float value) => Math.Max(0f, Math.Min(1f, value));
    }
}
