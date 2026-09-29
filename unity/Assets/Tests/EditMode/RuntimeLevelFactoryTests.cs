using NUnit.Framework;
using VectorTraffic3D.Board;
using VectorTraffic3D.Runtime;

namespace VectorTraffic3D.Tests
{
    public sealed class RuntimeLevelFactoryTests
    {
        [Test]
        public void RuntimeLevelFactory_ReturnsValidBoard()
        {
            var board = RuntimeLevelFactory.Create(20260929, 7, 7);

            Assert.That(board, Is.Not.Null);
            Assert.That(board.Vehicles.Count, Is.GreaterThan(0));
            Assert.That(board.Exits.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(board.TryValidateStaticState(out var error), Is.True, error);
        }

        [Test]
        public void RuntimeLevelFactory_IsDeterministicForSameSeed()
        {
            var first = RuntimeLevelFactory.Create(12345, 7, 7);
            var second = RuntimeLevelFactory.Create(12345, 7, 7);

            Assert.That(first.GetFullCanonicalStateHash(),
                Is.EqualTo(second.GetFullCanonicalStateHash()));
        }
    }
}
