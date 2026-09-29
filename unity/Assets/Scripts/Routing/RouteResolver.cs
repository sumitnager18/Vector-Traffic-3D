using System;
using System.Collections.Generic;
using System.Linq;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Vehicles;

namespace VectorTraffic3D.Routing
{
    public static class RouteResolver
    {
        /// <summary>
        /// Returns legal one-cell travel directions for the vehicle head.
        /// When a road graph is present, the graph is authoritative. On legacy/open-grid
        /// boards with no graph, the vehicle's current vector is the only legal direction.
        /// </summary>
        public static IReadOnlyList<Direction> GetLegalDirections(BoardState board, VehicleState vehicle)
        {
            if (vehicle == null || vehicle.IsExited)
                return Array.Empty<Direction>();

            if (board.RoadGraph == null || board.RoadGraph.IsEmpty)
                return new[] { vehicle.CurrentVector };

            return board.RoadGraph.GetOutgoingDirections(vehicle.HeadPosition);
        }

        public static bool IsDirectionLegal(BoardState board, VehicleState vehicle, Direction direction)
        {
            return GetLegalDirections(board, vehicle).Contains(direction);
        }

        public static bool TryGetNextPosition(BoardState board, VehicleState vehicle, Direction direction, out GridPosition next)
        {
            next = vehicle.HeadPosition.Offset(direction);
            if (board.RoadGraph == null || board.RoadGraph.IsEmpty)
                return direction == vehicle.CurrentVector;

            return board.RoadGraph.IsValidTransition(vehicle.HeadPosition, direction, out next);
        }
    }
}
