using NUnit.Framework;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Gates;
using VectorTraffic3D.Puzzle;
using VectorTraffic3D.Vehicles;

namespace VectorTraffic3D.Tests
{
    [TestFixture]
    public class CanonicalHashTests
    {
        [Test]
        public void IdenticalStates_ProduceIdenticalHashes()
        {
            var board1 = new BoardState(7, 7);
            board1.AddVehicle(new VehicleState(1, VehicleType.Bus, new GridPosition(3, 4), Direction.Up, Direction.Up));
            board1.AddGate(new VectorGate(1, new GridPosition(4, 2), Direction.Right));

            var board2 = new BoardState(7, 7);
            board2.AddVehicle(new VehicleState(1, VehicleType.Bus, new GridPosition(3, 4), Direction.Up, Direction.Up));
            board2.AddGate(new VectorGate(1, new GridPosition(4, 2), Direction.Right));

            Assert.AreEqual(board1.GetCanonicalStateHash(), board2.GetCanonicalStateHash());
        }

        [Test]
        public void DifferentOrientation_ProducesDifferentHash_EvenWithSameHeadAndVector()
        {
            // Two vehicles have same head (3, 3) and same vector (Right),
            // but one is oriented Up (occupies (3,3),(3,2)) and the other oriented Left (occupies (3,3),(4,3)).
            var board1 = new BoardState(7, 7);
            board1.AddVehicle(new VehicleState(1, VehicleType.Hatchback, new GridPosition(3, 3), Direction.Up, Direction.Right));

            var board2 = new BoardState(7, 7);
            board2.AddVehicle(new VehicleState(1, VehicleType.Hatchback, new GridPosition(3, 3), Direction.Left, Direction.Right));

            Assert.AreNotEqual(board1.GetCanonicalStateHash(), board2.GetCanonicalStateHash());
        }

        [Test]
        public void DifferentGateState_ProducesDifferentHash()
        {
            var board1 = new BoardState(7, 7);
            board1.AddGate(new VectorGate(1, new GridPosition(2, 2), Direction.Up));

            var board2 = new BoardState(7, 7);
            board2.AddGate(new VectorGate(1, new GridPosition(2, 2), Direction.Right));

            Assert.AreNotEqual(board1.GetCanonicalStateHash(), board2.GetCanonicalStateHash());
        }

        [Test]
        public void DifferentDestination_ProducesDifferentHash()
        {
            var board1 = new BoardState(7, 7);
            board1.AddVehicle(new VehicleState(1, VehicleType.Sedan, new GridPosition(2, 2), Direction.Up, Direction.Up, destinationId: 1));

            var board2 = new BoardState(7, 7);
            board2.AddVehicle(new VehicleState(1, VehicleType.Sedan, new GridPosition(2, 2), Direction.Up, Direction.Up, destinationId: 2));

            Assert.AreNotEqual(board1.GetCanonicalStateHash(), board2.GetCanonicalStateHash());
        }
    }
}
