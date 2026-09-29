using NUnit.Framework;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Vehicles;
using VectorTraffic3D.Generation;
using VectorTraffic3D.Puzzle;

namespace VectorTraffic3D.Tests
{
    [TestFixture]
    public class ReverseTransitionModelTests
    {
        [Test]
        public void UndoMovement_ReplaysToExactTargetState()
        {
            var start = new BoardState(6, 6);
            start.AddExit(new GridPosition(5, 2), 1);
            start.AddVehicle(new VehicleState(
                1, VehicleType.Hatchback,
                new GridPosition(2, 2),
                Direction.Right, Direction.Right, 1));

            var target = start.DeepClone();
            Assert.IsTrue(MoveValidator.TryExecuteStep(
                target, 1, Direction.Right, out _, out _));

            Assert.IsTrue(ReverseTransitionModel.TryUndoMovement(
                target, 1, Direction.Right, Direction.Right, out var predecessor));

            Assert.AreEqual(
                start.GetFullCanonicalStateHash(),
                predecessor.GetFullCanonicalStateHash());
        }
    }
}
