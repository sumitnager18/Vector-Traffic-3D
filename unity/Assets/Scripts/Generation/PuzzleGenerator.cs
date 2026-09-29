using System;
using System.Collections.Generic;
using System.Linq;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Gates;
using VectorTraffic3D.Vehicles;
namespace VectorTraffic3D.Generation
{
    public sealed class PuzzleGenerator
    {
        private readonly LevelValidator _validator = new LevelValidator();
        public GenerationResult Generate(int seed, int width = 7, int height = 7, int vehicleCount = 3, int gateCount = 1,
            int maxAttempts = 64, int minSolutionDepth = 5, float minInterestingness = 15f)
        {
            if (width < 5 || height < 5) throw new ArgumentOutOfRangeException(nameof(width));
            if (vehicleCount < 1 || vehicleCount > 5) throw new ArgumentOutOfRangeException(nameof(vehicleCount));
            if (gateCount < 0 || gateCount > 3) throw new ArgumentOutOfRangeException(nameof(gateCount));
            var result = new GenerationResult();
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int attemptSeed = unchecked(seed + attempt * 7919);
                var board = BuildCandidate(new DeterministicRandom(attemptSeed), width, height, vehicleCount, gateCount);
                var report = _validator.Validate(board, minSolutionDepth, minInterestingness);
                result.Attempts = attempt + 1;
                if (!report.IsValid) continue;
                result.Success = true;
                result.Level = new LevelDefinition(seed, attempt, board.DeepClone(), report.SolutionDepth,
                    report.ExploredStates, report.DifficultyScore, report.InterestingnessScore, true);
                return result;
            }
            result.FailureReason = $"No valid level found after {maxAttempts} deterministic attempts.";
            return result;
        }
        private static BoardState BuildCandidate(DeterministicRandom rng, int width, int height, int vehicleCount, int gateCount)
        {
            var board = new BoardState(width, height);
            BuildFullConnectedRoad(board);
            int midY = height / 2;
            board.AddExit(new GridPosition(0, midY), 1);
            board.AddExit(new GridPosition(width - 1, midY), 2);
            var occupied = new HashSet<GridPosition>();
            if (!PlaceVehicle(board, occupied, rng, 1, VehicleType.Sedan, 1, width, height))
                return new BoardState(width, height);
            for (int id = 2; id <= vehicleCount; id++)
            {
                var type = PickVehicleType(rng, id);
                int destination = id % 2 == 0 ? 2 : 1;
                if (!PlaceVehicle(board, occupied, rng, id, type, destination, width, height))
                    return new BoardState(width, height);
            }
            for (int id = 1; id <= gateCount; id++)
            {
                var empty = AllCells(width, height).Where(p => board.RoadGraph.IsConnected(p) &&
                    !occupied.Contains(p) && !board.Exits.ContainsKey(p) &&
                    board.Gates.Values.All(g => g.Position != p)).ToList();
                if (empty.Count == 0) break;
                board.AddGate(new VectorGate(id, rng.Pick(empty), RandomDirection(rng)));
            }
            return board;
        }
        private static void BuildFullConnectedRoad(BoardState board)
        {
            for (int x = 0; x < board.Width; x++)
                for (int y = 0; y < board.Height; y++)
                    board.AddRoadNode(new GridPosition(x, y));
            for (int x = 0; x < board.Width; x++)
                for (int y = 0; y < board.Height; y++)
                {
                    var p = new GridPosition(x, y);
                    if (x + 1 < board.Width) board.AddBidirectionalRoadSegment(p, new GridPosition(x + 1, y));
                    if (y + 1 < board.Height) board.AddBidirectionalRoadSegment(p, new GridPosition(x, y + 1));
                }
        }
        private static bool PlaceVehicle(BoardState board, HashSet<GridPosition> occupied, DeterministicRandom rng,
            int id, VehicleType type, int destination, int width, int height)
        {
            var orientations = new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
            for (int attempt = 0; attempt < 80; attempt++)
            {
                Direction orientation = rng.Pick(orientations);
                int length = type.GetDefaultLength();
                int minX = orientation == Direction.Left ? length - 1 : 0;
                int maxX = orientation == Direction.Right ? width - length : width - 1;
                int minY = orientation == Direction.Down ? length - 1 : 0;
                int maxY = orientation == Direction.Up ? height - length : height - 1;
                if (minX > maxX || minY > maxY) continue;
                var head = new GridPosition(rng.NextInt(minX, maxX + 1), rng.NextInt(minY, maxY + 1));
                var cells = VehicleFootprint.CalculateOccupiedCells(head, orientation, length);
                if (cells.Any(c => board.Exits.ContainsKey(c) || occupied.Contains(c))) continue;
                if (cells.Any(c => !board.RoadGraph.IsConnected(c))) continue;
                board.AddVehicle(new VehicleState(id, type, head, orientation, orientation, destination));
                foreach (var cell in cells) occupied.Add(cell);
                return true;
            }
            return false;
        }
        private static VehicleType PickVehicleType(DeterministicRandom rng, int id)
        {
            var choices = id % 3 == 0
                ? new[] { VehicleType.SUV, VehicleType.Van, VehicleType.Bus }
                : new[] { VehicleType.Hatchback, VehicleType.Sedan, VehicleType.SUV };
            return rng.Pick(choices);
        }
        private static Direction RandomDirection(DeterministicRandom rng) => (Direction)rng.NextInt(1, 5);
        private static IEnumerable<GridPosition> AllCells(int width, int height)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    yield return new GridPosition(x, y);
        }
    }
}
