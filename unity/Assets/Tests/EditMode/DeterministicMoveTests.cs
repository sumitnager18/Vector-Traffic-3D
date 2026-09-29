using System.Linq;
using NUnit.Framework;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Gates;
using VectorTraffic3D.Puzzle;
using VectorTraffic3D.Vehicles;

namespace VectorTraffic3D.Tests
{
    [TestFixture]
    public class DeterministicMoveTests
    {
        [Test]
        public void Vehicle_BlockedAtRearCell_CannotMove()
        {
            // 2-cell Hatchback: Head at (3, 3) facing Up -> body cell is at (3, 2).
            // Suppose vehicle attempts to move Right (vector = Right).
            // Candidate head: (4, 3). Candidate body: (4, 2).
            // Place blocker vehicle at (4, 2) [the rear/body cell] while (4, 3) is clear.
            var board = new BoardState(8, 8);

            var v1 = new VehicleState(1, VehicleType.Hatchback, new GridPosition(3, 3), Direction.Up, Direction.Right);
            // Blocker at (4, 2)
            var blocker = new VehicleState(2, VehicleType.Hatchback, new GridPosition(4, 2), Direction.Up, Direction.Up);

            board.AddVehicle(v1);
            board.AddVehicle(blocker);

            var occupancy = new OccupancySystem(8, 8);
            occupancy.TryRebuildOccupancy(board.Vehicles.Values, null, out _);

            var result = MoveValidator.ValidateSingleStep(board, occupancy, 1, out var candidate, out int blockedIdx, out var blockedPos);

            Assert.AreEqual(MoveResult.BlockedByVehicle, result);
            Assert.AreEqual(1, blockedIdx); // Blocked at body cell (index 1), NOT head (index 0)
            Assert.AreEqual(new GridPosition(4, 2), blockedPos);
        }

        [Test]
        public void Vehicle_BlockedByObstacleAtBodyCell_CannotMove()
        {
            // 3-cell Sedan: Head at (3, 3) facing Up -> cells (3, 3), (3, 2), (3, 1).
            // Moves Right -> candidate cells: (4, 3), (4, 2), (4, 1).
            // Obstacle placed at (4, 2) [middle body cell]. Head cell (4, 3) is clear!
            var board = new BoardState(8, 8);
            board.AddObstacle(new GridPosition(4, 2));

            var v = new VehicleState(1, VehicleType.Sedan, new GridPosition(3, 3), Direction.Up, Direction.Right);
            board.AddVehicle(v);

            var occupancy = new OccupancySystem(8, 8);
            board.Obstacles.ToList().ForEach(pos => occupancy.SetTerrain(pos, TerrainType.Obstacle));
            occupancy.TryRebuildOccupancy(board.Vehicles.Values, null, out _);

            var result = MoveValidator.ValidateSingleStep(board, occupancy, 1, out _, out int blockedIdx, out var blockedPos);

            Assert.AreEqual(MoveResult.BlockedByObstacle, result);
            Assert.AreEqual(1, blockedIdx); // Middle body cell blocked
            Assert.AreEqual(new GridPosition(4, 2), blockedPos);
        }

        [Test]
        public void Bus_BlockedByBoundary_BecauseBodyExtendsOutside()
        {
            // 4-cell Bus: Length 4. Board height = 6 (indices 0..5).
            // Bus at head (2, 2) facing Up (occupies (2,2), (2,1), (2,0), (2,-1 invalid)).
            // Place head at (2, 3) facing Up -> cells (2,3), (2,2), (2,1), (2,0).
            // Vehicle vector = Down -> candidate head at (2, 2). Candidate cells: (2,2), (2,1), (2,0), (2,-1).
            // Cell (2, -1) is below bottom boundary! Head (2, 2) is inside!
            var board = new BoardState(6, 6);
            var bus = new VehicleState(1, VehicleType.Bus, new GridPosition(2, 3), Direction.Up, Direction.Down);
            board.AddVehicle(bus);

            var occupancy = new OccupancySystem(6, 6);
            occupancy.TryRebuildOccupancy(board.Vehicles.Values, null, out _);

            var result = MoveValidator.ValidateSingleStep(board, occupancy, 1, out _, out int blockedIdx, out var blockedPos);

            Assert.AreEqual(MoveResult.BlockedByBoundary, result);
            Assert.AreEqual(3, blockedIdx); // Tail cell index 3 is out of bounds
            Assert.AreEqual(new GridPosition(2, -1), blockedPos);
        }

        [Test]
        public void Truck_BlockedByVehicleSeveralCellsBehindHead_CannotMove()
        {
            // 5-cell Delivery Truck: Length 5.
            // Head at (2, 5) facing Up -> occupies (2,5), (2,4), (2,3), (2,2), (2,1).
            // Vector = Right -> candidate cells: (3,5), (3,4), (3,3), (3,2), (3,1).
            // Blocker vehicle placed at (3, 1) [4 cells behind head!]. Head (3,5) and cells (3,4)..(3,2) are completely clear!
            var board = new BoardState(8, 8);
            var truck = new VehicleState(1, VehicleType.DeliveryTruck, new GridPosition(2, 5), Direction.Up, Direction.Right);
            var blocker = new VehicleState(2, VehicleType.Hatchback, new GridPosition(3, 1), Direction.Up, Direction.Up);

            board.AddVehicle(truck);
            board.AddVehicle(blocker);

            var occupancy = new OccupancySystem(8, 8);
            occupancy.TryRebuildOccupancy(board.Vehicles.Values, null, out _);

            var result = MoveValidator.ValidateSingleStep(board, occupancy, 1, out _, out int blockedIdx, out var blockedPos);

            Assert.AreEqual(MoveResult.BlockedByVehicle, result);
            Assert.AreEqual(4, blockedIdx); // Cell index 4 (tail cell) is blocked
            Assert.AreEqual(new GridPosition(3, 1), blockedPos);
        }

        [Test]
        public void ValidMovement_Succeeds_WhenCompleteFootprintIsClear()
        {
            var board = new BoardState(8, 8);
            var truck = new VehicleState(1, VehicleType.DeliveryTruck, new GridPosition(2, 5), Direction.Up, Direction.Right);
            board.AddVehicle(truck);

            var occupancy = new OccupancySystem(8, 8);
            occupancy.TryRebuildOccupancy(board.Vehicles.Values, null, out _);

            var result = MoveValidator.ValidateSingleStep(board, occupancy, 1, out var candidate, out int blockedIdx, out _);

            Assert.AreEqual(MoveResult.ValidAdvance, result);
            Assert.AreEqual(-1, blockedIdx);
            Assert.AreEqual(5, candidate.Length);
            Assert.AreEqual(new GridPosition(3, 5), candidate[0]);
            Assert.AreEqual(new GridPosition(3, 1), candidate[4]);
        }

        [Test]
        public void CompleteExit_ValidatesEntireFootprintLeavingBoard()
        {
            var board = new BoardState(7, 7);
            // Exit at top (3, 6)
            board.AddExit(new GridPosition(3, 6), 0);

            // Bus (4 cells) at (3, 5) pointing Up with vector Up
            var bus = new VehicleState(1, VehicleType.Bus, new GridPosition(3, 5), Direction.Up, Direction.Up);
            board.AddVehicle(bus);

            bool success = MoveValidator.CanCompleteExit(board, 1, out var steps, out var failure);

            Assert.IsTrue(success);
            Assert.AreEqual(MoveResult.ValidExitCompleted, failure);
            // Steps must include reaching exit + 3 clearing steps (total 4 steps for length 4)
            Assert.GreaterOrEqual(steps.Count, 4);
        }

        [Test]
        public void Exit_FailsOnDestinationMismatch()
        {
            var board = new BoardState(6, 6);
            // Exit expects Destination ID 2
            board.AddExit(new GridPosition(5, 2), 2);

            // Vehicle has Destination ID 1
            var v = new VehicleState(1, VehicleType.Hatchback, new GridPosition(4, 2), Direction.Right, Direction.Right, destinationId: 1);
            board.AddVehicle(v);

            bool success = MoveValidator.CanCompleteExit(board, 1, out _, out var failure);

            Assert.IsFalse(success);
            Assert.AreEqual(MoveResult.DestinationMismatch, failure);
        }
    }
}
