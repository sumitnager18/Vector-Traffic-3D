using System;
using System.Collections.Generic;
using VectorTraffic3D.Core;
using VectorTraffic3D.Gates;
using VectorTraffic3D.Vehicles;

namespace VectorTraffic3D.Board
{
    /// <summary>
    /// Authoritative occupancy matrix for the discrete grid.
    /// Strictly enforces precedence hierarchy:
    /// 1. Vehicle (Highest authority: dynamic blocker)
    /// 2. Gate (Directional filter: vehicle must match allowed vector to traverse)
    /// 3. Exit (Egress portal: vehicle must match destination to exit)
    /// 4. Obstacle (Static blocker: strictly impassable)
    /// 5. Road / Junction (Navigable lane)
    /// 6. Empty (Base grid)
    /// </summary>
    public class OccupancySystem
    {
        private readonly int _width;
        private readonly int _height;
        private readonly CellOccupancy[,] _grid;

        public int Width => _width;
        public int Height => _height;

        public OccupancySystem(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Grid dimensions must be positive integers.", nameof(width));

            _width = width;
            _height = height;
            _grid = new CellOccupancy[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _grid[x, y] = CellOccupancy.DefaultEmpty;
                }
            }
        }

        public bool IsInBounds(GridPosition pos)
        {
            return pos.X >= 0 && pos.X < _width && pos.Y >= 0 && pos.Y < _height;
        }

        public CellOccupancy GetCell(GridPosition pos)
        {
            if (!IsInBounds(pos))
            {
                return new CellOccupancy
                {
                    Terrain = TerrainType.Obstacle,
                    OccupyingVehicleId = null,
                    GateId = null,
                    ExitDestinationId = null
                };
            }
            return _grid[pos.X, pos.Y];
        }

        public void SetTerrain(GridPosition pos, TerrainType terrain, int? exitDestinationId = null)
        {
            if (!IsInBounds(pos)) return;
            _grid[pos.X, pos.Y].Terrain = terrain;
            _grid[pos.X, pos.Y].ExitDestinationId = exitDestinationId;
        }

        public void SetGate(GridPosition pos, int gateId)
        {
            if (!IsInBounds(pos)) return;
            _grid[pos.X, pos.Y].GateId = gateId;
        }

        public void ClearDynamicOccupancy()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _grid[x, y].OccupyingVehicleId = null;
                }
            }
        }

        public bool TryRebuildOccupancy(
            IEnumerable<VehicleState> vehicles,
            IEnumerable<VectorGate> gates,
            out string errorMessage)
        {
            errorMessage = null;

            // Clear vehicle dynamic references and gate bindings
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _grid[x, y].OccupyingVehicleId = null;
                    _grid[x, y].GateId = null;
                }
            }

            // Register gates (Precedence level 2)
            if (gates != null)
            {
                foreach (var gate in gates)
                {
                    if (!IsInBounds(gate.Position))
                    {
                        errorMessage = $"Gate {gate.Id} placed out of bounds at {gate.Position}.";
                        return false;
                    }
                    _grid[gate.Position.X, gate.Position.Y].GateId = gate.Id;
                }
            }

            // Register vehicles (Precedence level 1)
            if (vehicles != null)
            {
                foreach (var vehicle in vehicles)
                {
                    if (vehicle.IsExited) continue;

                    var cells = vehicle.GetOccupiedCells();
                    for (int i = 0; i < cells.Length; i++)
                    {
                        var cellPos = cells[i];

                        // 1. Boundary check on entire footprint
                        if (!IsInBounds(cellPos))
                        {
                            errorMessage = $"Vehicle {vehicle.Id} ({vehicle.Type}, length {vehicle.Length}) footprint cell {i} out of bounds at {cellPos}.";
                            return false;
                        }

                        var cell = _grid[cellPos.X, cellPos.Y];

                        // 2. Obstacle collision
                        if (cell.Terrain == TerrainType.Obstacle)
                        {
                            errorMessage = $"Vehicle {vehicle.Id} ({vehicle.Type}) footprint overlaps static obstacle at {cellPos}.";
                            return false;
                        }

                        // 3. Vehicle-on-vehicle overlap
                        if (cell.OccupyingVehicleId.HasValue)
                        {
                            errorMessage = $"Illegal overlap! Cell {cellPos} occupied by both Vehicle {cell.OccupyingVehicleId.Value} and Vehicle {vehicle.Id}.";
                            return false;
                        }

                        cell.OccupyingVehicleId = vehicle.Id;
                        _grid[cellPos.X, cellPos.Y] = cell;
                    }
                }
            }

            return true;
        }
    }
}
