using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorTraffic3D.Core;
using VectorTraffic3D.Gates;
using VectorTraffic3D.Routing;
using VectorTraffic3D.Vehicles;

namespace VectorTraffic3D.Board
{
    [Serializable]
    public class BoardState
    {
        public int Width { get; }
        public int Height { get; }
        public Dictionary<int, VehicleState> Vehicles { get; }
        public Dictionary<int, VectorGate> Gates { get; }
        public HashSet<GridPosition> Obstacles { get; }
        public Dictionary<GridPosition, int> Exits { get; }
        public RouteGraph RoadGraph { get; private set; }

        public BoardState(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Board dimensions must be positive.", nameof(width));

            Width = width;
            Height = height;
            Vehicles = new Dictionary<int, VehicleState>();
            Gates = new Dictionary<int, VectorGate>();
            Obstacles = new HashSet<GridPosition>();
            Exits = new Dictionary<GridPosition, int>();
            RoadGraph = new RouteGraph();
        }

        public bool HasRoadNetwork => RoadGraph != null && !RoadGraph.IsEmpty;

        public void SetRoadGraph(RouteGraph graph)
        {
            RoadGraph = graph ?? new RouteGraph();
            foreach (var exit in Exits.Keys.ToArray())
            {
                if (RoadGraph.TryGetNodeAt(exit, out var node))
                    node.Type = RoadNodeType.Exit;
            }
        }

        public int AddRoadNode(GridPosition position, RoadNodeType type = RoadNodeType.Normal)
        {
            if (Exits.ContainsKey(position))
                type = RoadNodeType.Exit;
            return RoadGraph.AddNode(position, type);
        }

        public void AddRoadSegment(GridPosition from, GridPosition to)
        {
            RoadGraph.AddDirectedSegment(from, to);
        }

        public void AddBidirectionalRoadSegment(GridPosition a, GridPosition b)
        {
            RoadGraph.AddBidirectionalSegment(a, b);
        }

        public void AddVehicle(VehicleState vehicle)
        {
            if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));
            Vehicles[vehicle.Id] = vehicle;
        }

        public void AddGate(VectorGate gate)
        {
            if (gate == null) throw new ArgumentNullException(nameof(gate));
            Gates[gate.Id] = gate;
        }

        public void AddObstacle(GridPosition position)
        {
            Obstacles.Add(position);
        }

        public void AddExit(GridPosition position, int destinationId = 0)
        {
            Exits[position] = destinationId;
            if (RoadGraph != null && RoadGraph.TryGetNodeAt(position, out var node))
                node.Type = RoadNodeType.Exit;
        }

        public BoardState DeepClone()
        {
            var clone = new BoardState(Width, Height)
            {
                RoadGraph = RoadGraph?.DeepClone() ?? new RouteGraph()
            };

            foreach (var kvp in Vehicles)
                clone.Vehicles[kvp.Key] = kvp.Value.Clone();

            foreach (var kvp in Gates)
                clone.Gates[kvp.Key] = kvp.Value.Clone();

            foreach (var obs in Obstacles)
                clone.Obstacles.Add(obs);

            foreach (var exit in Exits)
                clone.Exits[exit.Key] = exit.Value;

            return clone;
        }

        public bool IsSolved()
        {
            return Vehicles.Count > 0 && Vehicles.Values.All(v => v.IsExited);
        }

        /// <summary>
        /// Canonical dynamic state. Static road topology is deliberately excluded because
        /// it is immutable for a puzzle. Every dynamic variable capable of changing future
        /// legal moves is represented.
        /// </summary>
        public string GetCanonicalStateHash()
        {
            var sb = new StringBuilder();

            foreach (var v in Vehicles.Values.OrderBy(v => v.Id))
            {
                if (v.IsExited)
                {
                    sb.Append($"V{v.Id}:EXITED;");
                }
                else
                {
                    sb.Append($"V{v.Id}:{v.HeadPosition.X},{v.HeadPosition.Y}:T{(int)v.Type}:O{(int)v.Orientation}:L{v.Length}:W{v.Width}:V{(int)v.CurrentVector}:D{v.DestinationId};");
                }
            }

            foreach (var g in Gates.Values.OrderBy(g => g.Id))
                sb.Append($"G{g.Id}:P{g.Position.X},{g.Position.Y}:D{(int)g.AllowedDirection};");

            return sb.ToString();
        }

        /// <summary>
        /// Validates static puzzle invariants before the board is handed to gameplay or a solver.
        /// This prevents generated levels from starting with a vehicle partly off-road or
        /// occupying an exit/obstacle illegally.
        /// </summary>
        public bool TryValidateStaticState(out string errorMessage)
        {
            errorMessage = null;

            if (Width <= 0 || Height <= 0)
            {
                errorMessage = "Board dimensions must be positive.";
                return false;
            }

            foreach (var exit in Exits)
            {
                if (exit.Key.X < 0 || exit.Key.X >= Width || exit.Key.Y < 0 || exit.Key.Y >= Height)
                {
                    errorMessage = $"Exit {exit.Key} is outside the board.";
                    return false;
                }

                if (HasRoadNetwork && !RoadGraph.IsConnected(exit.Key))
                {
                    errorMessage = $"Exit {exit.Key} is not connected to the road graph.";
                    return false;
                }
            }

            foreach (var obstacle in Obstacles)
            {
                if (obstacle.X < 0 || obstacle.X >= Width || obstacle.Y < 0 || obstacle.Y >= Height)
                {
                    errorMessage = $"Obstacle {obstacle} is outside the board.";
                    return false;
                }
            }

            foreach (var vehicle in Vehicles.Values)
            {
                if (vehicle.IsExited)
                    continue;

                foreach (var cell in vehicle.GetOccupiedCells())
                {
                    if (cell.X < 0 || cell.X >= Width || cell.Y < 0 || cell.Y >= Height)
                    {
                        errorMessage = $"Vehicle {vehicle.Id} footprint is outside the board at {cell}.";
                        return false;
                    }

                    if (Obstacles.Contains(cell))
                    {
                        errorMessage = $"Vehicle {vehicle.Id} overlaps obstacle at {cell}.";
                        return false;
                    }

                    if (HasRoadNetwork && !RoadGraph.IsConnected(cell))
                    {
                        errorMessage = $"Vehicle {vehicle.Id} starts off-road at {cell}.";
                        return false;
                    }
                }
            }

            var occupancy = new OccupancySystem(Width, Height);
            if (!occupancy.TryRebuildOccupancy(Vehicles.Values, Gates.Values, out var occupancyError))
            {
                errorMessage = occupancyError;
                return false;
            }

            return true;
        }

        public string GetFullCanonicalStateHash()
        {
            return GetCanonicalStateHash() + "|R:" + (RoadGraph?.GetCanonicalTopologyHash() ?? string.Empty);
        }
    }
}
