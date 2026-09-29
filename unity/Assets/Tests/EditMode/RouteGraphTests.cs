using NUnit.Framework;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Puzzle;
using VectorTraffic3D.Routing;
using VectorTraffic3D.Vehicles;

namespace VectorTraffic3D.Tests
{
    [TestFixture]
    public class RouteGraphTests
    {
        [Test]
        public void BidirectionalSegment_ProducesBothDirections()
        {
            var graph = new RouteGraph();
            graph.AddBidirectionalSegment(
                new GridPosition(1, 1),
                new GridPosition(2, 1));

            Assert.IsTrue(graph.IsConnected(new GridPosition(1, 1)));
            Assert.IsTrue(graph.IsConnected(new GridPosition(2, 1)));
            Assert.IsTrue(graph.HasOutgoingDirection(
                new GridPosition(1, 1),
                Direction.Right));
            Assert.IsTrue(graph.HasOutgoingDirection(
                new GridPosition(2, 1),
                Direction.Left));
        }

        [Test]
        public void Junction_ExposesMultipleOutgoingDirections()
        {
            var graph = new RouteGraph();
            var center = new GridPosition(2, 2);

            graph.AddBidirectionalSegment(center, new GridPosition(1, 2));
            graph.AddBidirectionalSegment(center, new GridPosition(3, 2));
            graph.AddBidirectionalSegment(center, new GridPosition(2, 3));

            Assert.IsTrue(graph.IsJunction(center));
            Assert.AreEqual(3, graph.GetOutgoingDirections(center).Count);
        }

        [Test]
        public void DirectedSegment_RejectsIllegalReverseTransition()
        {
            var graph = new RouteGraph();
            graph.AddDirectedSegment(
                new GridPosition(1, 1),
                new GridPosition(2, 1));

            Assert.IsTrue(graph.HasOutgoingDirection(
                new GridPosition(1, 1),
                Direction.Right));
            Assert.IsFalse(graph.HasOutgoingDirection(
                new GridPosition(2, 1),
                Direction.Left));
        }

        [Test]
        public void MultiCellTurn_RequiresEntireRotatedFootprintToBeOnRoad()
        {
            var board = new BoardState(6, 6);
            board.AddBidirectionalRoadSegment(
                new GridPosition(0, 1),
                new GridPosition(1, 1));
            board.AddBidirectionalRoadSegment(
                new GridPosition(1, 1),
                new GridPosition(1, 2));

            var vehicle = new VehicleState(
                1,
                VehicleType.Hatchback,
                new GridPosition(1, 1),
                Direction.Right,
                Direction.Right);
            board.AddVehicle(vehicle);

            var result = MoveValidator.GetLegalStepDirections(board, 1);

            CollectionAssert.Contains(result, Direction.Up);
            CollectionAssert.DoesNotContain(result, Direction.Right);
        }

        [Test]
        public void MultiCellTurn_IsRejectedIfRotatedBodyLeavesRoad()
        {
            var board = new BoardState(6, 6);
            board.AddBidirectionalRoadSegment(
                new GridPosition(0, 1),
                new GridPosition(1, 1));
            board.AddBidirectionalRoadSegment(
                new GridPosition(1, 1),
                new GridPosition(1, 2));

            var vehicle = new VehicleState(
                1,
                VehicleType.Sedan,
                new GridPosition(1, 1),
                Direction.Right,
                Direction.Right);
            board.AddVehicle(vehicle);

            var result = MoveValidator.GetLegalStepDirections(board, 1);

            CollectionAssert.DoesNotContain(result, Direction.Up);
        }

        [Test]
        public void TurnTransition_UpdatesOrientationAndVector()
        {
            var board = new BoardState(6, 6);
            board.AddBidirectionalRoadSegment(
                new GridPosition(0, 1),
                new GridPosition(1, 1));
            board.AddBidirectionalRoadSegment(
                new GridPosition(1, 1),
                new GridPosition(1, 2));

            var vehicle = new VehicleState(
                1,
                VehicleType.Hatchback,
                new GridPosition(1, 1),
                Direction.Right,
                Direction.Right);
            board.AddVehicle(vehicle);

            var success = MoveValidator.TryExecuteStep(
                board,
                1,
                Direction.Up,
                out _,
                out var result);

            Assert.IsTrue(success);
            Assert.AreEqual(MoveResult.ValidAdvance, result);
            Assert.AreEqual(Direction.Up, vehicle.Orientation);
            Assert.AreEqual(Direction.Up, vehicle.CurrentVector);
            Assert.AreEqual(new GridPosition(1, 2), vehicle.HeadPosition);
        }
        [Test]
        public void StaticValidation_RejectsVehicleStartingOffRoad()
        {
            var board = new BoardState(6, 6);
            board.AddBidirectionalRoadSegment(
                new GridPosition(2, 2),
                new GridPosition(3, 2));

            board.AddVehicle(new VehicleState(
                1,
                VehicleType.Hatchback,
                new GridPosition(2, 2),
                Direction.Right,
                Direction.Right));

            Assert.IsFalse(board.TryValidateStaticState(out var error));
            StringAssert.Contains("starts off-road", error);
        }

        [Test]
        public void StaticValidation_RejectsOverlappingVehicles()
        {
            var board = new BoardState(8, 8);
            board.AddVehicle(new VehicleState(
                1,
                VehicleType.Hatchback,
                new GridPosition(3, 3),
                Direction.Right,
                Direction.Right));
            board.AddVehicle(new VehicleState(
                2,
                VehicleType.Hatchback,
                new GridPosition(4, 3),
                Direction.Left,
                Direction.Left));

            Assert.IsFalse(board.TryValidateStaticState(out var error));
            StringAssert.Contains("Illegal overlap", error);
        }

    }
}
