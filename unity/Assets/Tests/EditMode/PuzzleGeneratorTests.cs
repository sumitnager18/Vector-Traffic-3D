using NUnit.Framework;
using VectorTraffic3D.Generation;
namespace VectorTraffic3D.Tests
{
    [TestFixture]
    public class PuzzleGeneratorTests
    {
        [Test]
        public void Generate_IsDeterministicForSameSeed()
        {
            var g = new PuzzleGenerator();
            var a = g.Generate(12345, 7, 7, 3, 1, 64);
            var b = g.Generate(12345, 7, 7, 3, 1, 64);
            Assert.IsTrue(a.Success, a.FailureReason);
            Assert.IsTrue(b.Success, b.FailureReason);
            Assert.AreEqual(a.Level.Board.GetFullCanonicalStateHash(), b.Level.Board.GetFullCanonicalStateHash());
            Assert.AreEqual(a.Level.SolutionDepth, b.Level.SolutionDepth);
            Assert.AreEqual(a.Level.DifficultyScore, b.Level.DifficultyScore, .001f);
            Assert.AreEqual(a.Level.InterestingnessScore, b.Level.InterestingnessScore, .001f);
        }
        [Test]
        public void Generate_ProducesSolverValidatedLevel()
        {
            var g = new PuzzleGenerator();
            var r = g.Generate(9876, 7, 7, 3, 1, 96);
            Assert.IsTrue(r.Success, r.FailureReason);
            Assert.IsTrue(r.Level.IsValidated);
            Assert.GreaterOrEqual(r.Level.SolutionDepth, 5);
            Assert.GreaterOrEqual(r.Level.InterestingnessScore, 15f);
            Assert.IsTrue(r.Level.Board.TryValidateStaticState(out var error), error);
        }
        [Test]
        public void Generate_DifferentSeedsCanProduceDifferentLogicalStates()
        {
            var g = new PuzzleGenerator();
            var a = g.Generate(1001, 7, 7, 2, 0, 96);
            var b = g.Generate(2002, 7, 7, 2, 0, 96);
            Assert.IsTrue(a.Success, a.FailureReason);
            Assert.IsTrue(b.Success, b.FailureReason);
            Assert.AreNotEqual(a.Level.Board.GetFullCanonicalStateHash(), b.Level.Board.GetFullCanonicalStateHash());
        }
    }
}
