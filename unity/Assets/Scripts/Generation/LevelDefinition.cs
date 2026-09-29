using System;
using VectorTraffic3D.Board;
namespace VectorTraffic3D.Generation
{
    [Serializable]
    public sealed class LevelDefinition
    {
        public int Seed { get; }
        public int GenerationAttempt { get; }
        public BoardState Board { get; }
        public int SolutionDepth { get; }
        public int ExploredStates { get; }
        public float DifficultyScore { get; }
        public float InterestingnessScore { get; }
        public bool IsValidated { get; }
        public LevelDefinition(int seed, int generationAttempt, BoardState board, int solutionDepth, int exploredStates, float difficultyScore, float interestingnessScore, bool isValidated)
        {
            Seed = seed; GenerationAttempt = generationAttempt; Board = board;
            SolutionDepth = solutionDepth; ExploredStates = exploredStates;
            DifficultyScore = difficultyScore; InterestingnessScore = interestingnessScore; IsValidated = isValidated;
        }
    }
    public sealed class GenerationResult
    {
        public bool Success { get; internal set; }
        public LevelDefinition Level { get; internal set; }
        public string FailureReason { get; internal set; }
        public int Attempts { get; internal set; }
    }
}
