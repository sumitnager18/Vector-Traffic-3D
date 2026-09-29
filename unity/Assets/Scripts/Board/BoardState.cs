using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectorTraffic3D.Core;
using VectorTraffic3D.Gates;
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
        public Dictionary<GridPosition, int> Exits { get; } // Position -> DestinationId (0 = wildcard)

        public BoardState(int width, int height)
        {
            Width = width;
            Height = height;
            Vehicles = new Dictionary<int, VehicleState>();
            Gates = new Dictionary<int, VectorGate>();
            Obstacles = new HashSet<GridPosition>();
            Exits = new Dictionary<GridPosition, int>();
        }

        public void AddVehicle(VehicleState vehicle)
        {
            Vehicles[vehicle.Id] = vehicle;
        }

        public void AddGate(VectorGate gate)
        {
            Gates[gate.Id] = gate;
        }

        public void AddObstacle(GridPosition position)
        {
            Obstacles.Add(position);
        }

        public void AddExit(GridPosition position, int destinationId = 0)
        {
            Exits[position] = destinationId;
        }

        public BoardState DeepClone()
        {
            var clone = new BoardState(Width, Height);
            foreach (var kvp in Vehicles)
            {
                clone.Vehicles[kvp.Key] = kvp.Value.Clone();
            }
            foreach (var kvp in Gates)
            {
                clone.Gates[kvp.Key] = kvp.Value.Clone();
            }
            foreach (var obs in Obstacles)
            {
                clone.Obstacles.Add(obs);
            }
            foreach (var exit in Exits)
            {
                clone.Exits[exit.Key] = exit.Value;
            }
            return clone;
        }

        public bool IsSolved()
        {
            return Vehicles.Count > 0 && Vehicles.Values.All(v => v.IsExited);
        }

        /// <summary>
        /// Produces a canonical state hash that incorporates every state variable that can affect
        /// future legal moves: vehicle exited state, head position, orientation, length, current vector,
        /// destination ID, and gate allowed directions.
        /// </summary>
        public string GetCanonicalStateHash()
        {
            var sb = new StringBuilder();

            // Order by ID to ensure canonical ordering
            foreach (var v in Vehicles.Values.OrderBy(v => v.Id))
            {
                if (v.IsExited)
                {
                    sb.Append($"V{v.Id}:EXITED;");
                }
                else
                {
                    sb.Append($"V{v.Id}:{v.HeadPosition.X},{v.HeadPosition.Y}:O{(int)v.Orientation}:L{v.Length}:V{(int)v.CurrentVector}:D{v.DestinationId};");
                }
            }

            foreach (var g in Gates.Values.OrderBy(g => g.Id))
            {
                sb.Append($"G{g.Id}:D{(int)g.AllowedDirection};");
            }

            return sb.ToString();
        }
    }
}
