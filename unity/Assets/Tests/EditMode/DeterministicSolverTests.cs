using NUnit.Framework;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Gates;
using VectorTraffic3D.Solver;
using VectorTraffic3D.Vehicles;

namespace VectorTraffic3D.Tests
{
    [TestFixture]
    public class DeterministicSolverTests
    {
        [Test]
        public void Solve_SingleVehiclePuzzle_UsesExplicitMovementThenExit()
        {
            var board = new BoardState(5, 5);
            board.AddExit(new GridPosition(4, 2), 0);

            var v1 = new VehicleState(
                1,
                VehicleType.Hatchback,
                new GridPosition(3, 2),
                Direction.Right,
                Direction.Right);
            board.AddVehicle(v1);

            var solver = new DeterministicSolver();
            var result = solver.Solve(board);

            Assert.IsTrue(result.IsSolvable);
            Assert.AreEqual(2, result.SolutionDepth);
            Assert.AreEqual(1, result.Steps[0].VehicleId);
            Assert.IsFalse(result.Steps[0].IsExitAction);
            Assert.AreEqual(1, result.Steps[1].VehicleId);
            Assert.IsTrue(result.Steps[1].IsExitAction);
        }

        [Test]
        public void Solve_DependencyChainPuzzle_UsesIntermediateStates()
        {
            var board = new BoardState(7, 7);

            board.AddExit(new GridPosition(3, 6), 0);
            board.AddExit(new GridPosition(6, 2), 0);

            var v1 = new VehicleState(
                1,
                VehicleType.Bus,
                new GridPosition(3, 4),
                Direction.Up,
                Direction.Up);

            var v2 = new VehicleState(
                2,
                VehicleType.Hatchback,
                new GridPosition(2, 2),
                Direction.Right,
                Direction.Right);

            board.AddVehicle(v1);
            board.AddVehicle(v2);

            var solver = new DeterministicSolver();
            var result = solver.Solve(board);

            Assert.IsTrue(result.IsSolvable);
            Assert.AreEqual(8, result.SolutionDepth);

            Assert.AreEqual(1, result.Steps[0].VehicleId);
            Assert.AreEqual(1, result.Steps[1].VehicleId);
            Assert.AreEqual(1, result.Steps[2].VehicleId);
            Assert.IsTrue(result.Steps[2].IsExitAction);

            Assert.AreEqual(2, result.Steps[3].VehicleId);
            Assert.AreEqual(2, result.Steps[7].VehicleId);
            Assert.IsTrue(result.Steps[7].IsExitAction);
        }

        [Test]
        public void Solve_VectorGatePuzzle_CyclesGateThenTraversesThenExits()
        {
            var board = new BoardState(6, 6);
            board.AddExit(new GridPosition(5, 2), 0);
            board.AddGate(new VectorGate(
                1,
                new GridPosition(4, 2),
                Direction.Up));

            var v = new VehicleState(
                1,
                VehicleType.Hatchback,
                new GridPosition(3, 2),
                Direction.Right,
                Direction.Right);
            board.AddVehicle(v);

            var solver = new DeterministicSolver();
            var result = solver.Solve(board);

            Assert.IsTrue(result.IsSolvable);
            Assert.AreEqual(4, result.SolutionDepth);
            Assert.AreEqual(1, result.Steps[0].ToggledGateId);
            Assert.IsFalse(result.Steps[1].IsExitAction);
            Assert.IsFalse(result.Steps[2].IsExitAction);
            Assert.IsTrue(result.Steps[3].IsExitAction);
        }

        [Test]
        public void Solve_RouteGraphRequiresIntermediateTurn()
        {
            var board = new BoardState(6, 6);

            board.AddBidirectionalRoadSegment(
                new GridPosition(0, 1),
                new GridPosition(1, 1));
            board.AddBidirectionalRoadSegment(
                new GridPosition(1, 1),
                new GridPosition(1, 2));
            board.AddBidirectionalRoadSegment(
                new GridPosition(1, 2),
                new GridPosition(1, 3));
            board.AddExit(new GridPosition(1, 3), 0);

            var v = new VehicleState(
                1,
                VehicleType.Hatchback,
                new GridPosition(1, 1),
                Direction.Right,
                Direction.Up);

            board.AddVehicle(v);

            var solver = new DeterministicSolver();
            var result = solver.Solve(board);

            Assert.IsTrue(result.IsSolvable);
            Assert.GreaterOrEqual(result.SolutionDepth, 4);
        }
    }
}
