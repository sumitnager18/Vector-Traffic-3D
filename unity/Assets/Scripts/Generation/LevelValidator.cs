using VectorTraffic3D.Board;
using VectorTraffic3D.Solver;
namespace VectorTraffic3D.Generation
{
    public sealed class LevelValidationReport
    {
        public bool IsValid { get; internal set; }
        public bool StaticStateValid { get; internal set; }
        public bool Solvable { get; internal set; }
        public bool NonTrivial { get; internal set; }
        public bool InterestingEnough { get; internal set; }
        public int SolutionDepth { get; internal set; }
        public int ExploredStates { get; internal set; }
        public float DifficultyScore { get; internal set; }
        public float InterestingnessScore { get; internal set; }
        public string ErrorMessage { get; internal set; }
    }
    public sealed class LevelValidator
    {
        private readonly DifficultyAnalyzer _difficulty = new DifficultyAnalyzer();
        private readonly InterestingnessAnalyzer _interestingness = new InterestingnessAnalyzer();
        public LevelValidationReport Validate(BoardState board, int minSolutionDepth = 5, float minInterestingness = 15f, int solverStateLimit = 50000)
        {
            var report = new LevelValidationReport();
            if (board == null) { report.ErrorMessage = "Board is null."; return report; }
            report.StaticStateValid = board.TryValidateStaticState(out var staticError);
            if (!report.StaticStateValid) { report.ErrorMessage = staticError; return report; }
            var result = new DeterministicSolver(solverStateLimit).Solve(board);
            report.Solvable = result.IsSolvable;
            report.SolutionDepth = result.SolutionDepth;
            report.ExploredStates = result.ExploredStatesCount;
            if (!report.Solvable)
            {
                report.ErrorMessage = result.ExploredStatesCount >= solverStateLimit
                    ? "Solver state limit reached before a solution was found."
                    : "Generated puzzle is unsolvable.";
                return report;
            }
            report.DifficultyScore = _difficulty.Analyze(board, result);
            report.InterestingnessScore = _interestingness.Analyze(board, result);
            report.NonTrivial = report.SolutionDepth >= minSolutionDepth;
            report.InterestingEnough = report.InterestingnessScore >= minInterestingness;
            report.IsValid = report.NonTrivial && report.InterestingEnough;
            if (!report.IsValid)
                report.ErrorMessage = !report.NonTrivial ? "Puzzle is solvable but too trivial." : "Puzzle lacks enough interaction.";
            return report;
        }
    }
}
