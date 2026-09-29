using System.Collections.Generic;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Generation;
using VectorTraffic3D.Gates;
using VectorTraffic3D.Vehicles;

namespace VectorTraffic3D.Runtime
{
    public static class RuntimeLevelFactory
    {
        public static BoardState Create(int seed, int width, int height)
        {
            var generated = new PuzzleGenerator().Generate(
                seed,
                width,
                height,
                vehicleCount: 3,
                gateCount: 1,
                maxAttempts: 96,
                minSolutionDepth: 4,
                minInterestingness: 10f);

            if (generated.Success && generated.Level != null)
                return generated.Level.Board;

            return CreateFallback(width, height);
        }

        private static BoardState CreateFallback(int width, int height)
        {
            var board = new BoardState(width, height);
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                board.AddRoadNode(new GridPosition(x, y));

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var p = new GridPosition(x, y);
                if (x + 1 < width) board.AddBidirectionalRoadSegment(p, new GridPosition(x + 1, y));
                if (y + 1 < height) board.AddBidirectionalRoadSegment(p, new GridPosition(x, y + 1));
            }

            var mid = height / 2;
            board.AddExit(new GridPosition(0, mid), 1);
            board.AddExit(new GridPosition(width - 1, mid), 2);
            board.AddVehicle(new VehicleState(1, VehicleType.Sedan,
                new GridPosition(2, mid), Direction.Right, Direction.Right, 2));
            board.AddVehicle(new VehicleState(2, VehicleType.SUV,
                new GridPosition(4, mid - 2), Direction.Up, Direction.Up, 1));
            board.AddVehicle(new VehicleState(3, VehicleType.Hatchback,
                new GridPosition(3, mid + 2), Direction.Down, Direction.Down, 2));
            board.AddGate(new VectorGate(1, new GridPosition(width - 2, mid),
                Direction.Right));
            return board;
        }
    }
}
