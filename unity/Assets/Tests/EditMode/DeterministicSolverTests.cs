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
        public void Solve_SingleVehiclePuzzle_SolvesInOneStep()
        {
            var board = new BoardState(5, 5);
            board.AddExit(new GridPosition(4, 2), 0);

            var v1 = new VehicleState(1, VehicleType.Hatchback, new GridPosition(3, 2), Direction.Right, Direction.Right);
            board.AddVehicle(v1);

            var solver = new DeterministicSolver();
            var result = solver.Solve(board);

            Assert.IsTrue(result.IsSolvable);
            Assert.AreEqual(1, result.SolutionDepth);
            Assert.AreEqual(1, result.Steps[0].VehicleId);
        }

        [Test]
        public void Solve_DependencyChainPuzzle_SolvesInCorrectSequence()
        {
            // Level: Vehicle 1 (Bus, 4 cells) blocks Vehicle 2 (Hatchback, 2 cells).
            // Vehicle 1 must clear first so Vehicle 2 can reach its exit.
            var board = new BoardState(7, 7);

            // Exit for V1 at top (3, 6)
            board.AddExit(new GridPosition(3, 6), 0);
            // Exit for V2 at right (6, 2)
            board.AddExit(new GridPosition(6, 2), 0);

            // V1 (Bus) at (3, 4) pointing Up (occupies (3,4), (3,3), (3,2), (3,1))
            var v1 = new VehicleState(1, VehicleType.Bus, new GridPosition(3, 4), Direction.Up, Direction.Up);

            // V2 (Hatchback) at (2, 2) pointing Right (occupies (2,2), (1,2))
            // Notice: V2 needs to cross (3, 2) which is occupied by V1's body!
            var v2 = new VehicleState(2, VehicleType.Hatchback, new GridPosition(2, 2), Direction.Right, Direction.Right);

            board.AddVehicle(v1);
            board.AddVehicle(v2);

            var solver = new DeterministicSolver();
            var result = solver.Solve(board);

            Assert.IsTrue(result.IsSolvable);
            Assert.AreEqual(2, result.SolutionDepth);
            Assert.AreEqual(1, result.Steps[0].VehicleId); // V1 must exit first
            Assert.AreEqual(2, result.Steps[1].VehicleId); // V2 can now exit
        }

        [Test]
        public void Solve_VectorGatePuzzle_CyclesGateThenExits()
        {
            var board = new BoardState(6, 6);
            board.AddExit(new GridPosition(5, 2), 0);

            // Gate at (4, 2) initially pointing Up (blocks Right-facing vehicle)
            board.AddGate(new VectorGate(1, new GridPosition(4, 2), Direction.Up));

            // Vehicle at (3, 2) pointing Right
            var v = new VehicleState(1, VehicleType.Hatchback, new GridPosition(3, 2), Direction.Right, Direction.Right);
            board.AddVehicle(v);

            var solver = new DeterministicSolver();
            var result = solver.Solve(board);

            Assert.IsTrue(result.IsSolvable);
            // Must cycle gate Up -> Right, then vehicle exits!
            Assert.AreEqual(2, result.SolutionDepth);
            Assert.AreEqual(1, result.Steps[0].ToggledGateId);
            Assert.AreEqual(1, result.Steps[1].VehicleId);
        }
    }
}
