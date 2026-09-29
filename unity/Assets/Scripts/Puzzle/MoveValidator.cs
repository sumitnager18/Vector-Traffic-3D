using System;
using System.Collections.Generic;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Routing;
using VectorTraffic3D.Vehicles;

namespace VectorTraffic3D.Puzzle
{
    public enum MoveResult
    {
        ValidAdvance,
        ValidExitCompleted,
        BlockedByVehicle,
        BlockedByObstacle,
        BlockedByGate,
        BlockedByBoundary,
        BlockedByRoad,
        InvalidRoute,
        DestinationMismatch,
        VehicleAlreadyExited,
        VehicleNotFound
    }

    /// <summary>
    /// Authoritative movement contract for gameplay and solver.
    /// Every transition validates the complete multi-cell footprint.
    /// When a RouteGraph exists, it is authoritative for legal head transitions.
    /// </summary>
    public static class MoveValidator
    {
        public static MoveResult ValidateSingleStep(
            BoardState board,
            OccupancySystem occupancy,
            int vehicleId,
            out GridPosition[] candidateFootprint,
            out int blockedCellIndex,
            out GridPosition blockedCellPos)
        {
            if (!board.Vehicles.TryGetValue(vehicleId, out var vehicle))
            {
                candidateFootprint = Array.Empty<GridPosition>();
                blockedCellIndex = -1;
                blockedCellPos = default;
                return MoveResult.VehicleNotFound;
            }

            return ValidateStep(
                board,
                occupancy,
                vehicleId,
                vehicle.CurrentVector,
                out candidateFootprint,
                out blockedCellIndex,
                out blockedCellPos);
        }

        /// <summary>
        /// Validates one logical head transition in the requested direction.
        /// A turn is legal only when the route graph exposes that outgoing direction.
        /// The candidate vehicle orientation becomes the travel direction, so the entire
        /// footprint is re-evaluated in the new orientation.
        /// </summary>
        public static MoveResult ValidateStep(
            BoardState board,
            OccupancySystem occupancy,
            int vehicleId,
            Direction travelDirection,
            out GridPosition[] candidateFootprint,
            out int blockedCellIndex,
            out GridPosition blockedCellPos)
        {
            candidateFootprint = Array.Empty<GridPosition>();
            blockedCellIndex = -1;
            blockedCellPos = default;

            if (!board.Vehicles.TryGetValue(vehicleId, out var vehicle))
                return MoveResult.VehicleNotFound;

            if (vehicle.IsExited)
                return MoveResult.VehicleAlreadyExited;

            if (travelDirection == Direction.None)
                return MoveResult.InvalidRoute;

            if (!RouteResolver.TryGetNextPosition(board, vehicle, travelDirection, out var newHeadPos))
                return MoveResult.InvalidRoute;

            var candidate = VehicleFootprint.CalculateOccupiedCells(
                newHeadPos,
                travelDirection,
                vehicle.Length);

            candidateFootprint = candidate;

            bool headAtExit = board.Exits.TryGetValue(newHeadPos, out int exitDestinationId);

            for (int i = 0; i < candidate.Length; i++)
            {
                var cell = candidate[i];

                if (i == 0 && headAtExit)
                {
                    if (vehicle.DestinationId != 0 &&
                        exitDestinationId != 0 &&
                        vehicle.DestinationId != exitDestinationId)
                    {
                        blockedCellIndex = 0;
                        blockedCellPos = cell;
                        return MoveResult.DestinationMismatch;
                    }
                    continue;
                }

                if (!occupancy.IsInBounds(cell))
                {
                    blockedCellIndex = i;
                    blockedCellPos = cell;
                    return MoveResult.BlockedByBoundary;
                }

                if (board.Obstacles.Contains(cell))
                {
                    blockedCellIndex = i;
                    blockedCellPos = cell;
                    return MoveResult.BlockedByObstacle;
                }

                if (board.HasRoadNetwork && !board.RoadGraph.IsConnected(cell))
                {
                    blockedCellIndex = i;
                    blockedCellPos = cell;
                    return MoveResult.BlockedByRoad;
                }

                if (board.Gates.TryGetValue(GetGateIdAt(board, cell), out var gate) &&
                    gate.AllowedDirection != travelDirection)
                {
                    blockedCellIndex = i;
                    blockedCellPos = cell;
                    return MoveResult.BlockedByGate;
                }

                var occupant = occupancy.GetCell(cell).OccupyingVehicleId;
                if (occupant.HasValue && occupant.Value != vehicle.Id)
                {
                    blockedCellIndex = i;
                    blockedCellPos = cell;
                    return MoveResult.BlockedByVehicle;
                }
            }

            return MoveResult.ValidAdvance;
        }

        /// <summary>
        /// Returns every legal one-cell direction from the vehicle's current head.
        /// </summary>
        public static IReadOnlyList<Direction> GetLegalStepDirections(BoardState board, int vehicleId)
        {
            if (!board.Vehicles.TryGetValue(vehicleId, out var vehicle) || vehicle.IsExited)
                return Array.Empty<Direction>();

            if (board.Exits.ContainsKey(vehicle.HeadPosition))
                return Array.Empty<Direction>();

            var occupancy = new OccupancySystem(board.Width, board.Height);
            if (!occupancy.TryRebuildOccupancy(board.Vehicles.Values, board.Gates.Values, out _))
                return Array.Empty<Direction>();

            var legal = new List<Direction>();
            foreach (var direction in RouteResolver.GetLegalDirections(board, vehicle))
            {
                var result = ValidateStep(board, occupancy, vehicleId, direction, out _, out _, out _);
                if (result == MoveResult.ValidAdvance)
                    legal.Add(direction);
            }

            return legal;
        }

        /// <summary>
        /// Commits exactly one validated movement transition.
        /// No visual animation or physics is involved in this logical operation.
        /// </summary>
        public static bool TryExecuteStep(
            BoardState board,
            int vehicleId,
            Direction travelDirection,
            out GridPosition[] candidateFootprint,
            out MoveResult result)
        {
            candidateFootprint = Array.Empty<GridPosition>();

            if (!board.Vehicles.ContainsKey(vehicleId))
            {
                result = MoveResult.VehicleNotFound;
                return false;
            }

            if (board.Exits.ContainsKey(board.Vehicles[vehicleId].HeadPosition))
            {
                result = MoveResult.ValidExitCompleted;
                return false;
            }

            var occupancy = new OccupancySystem(board.Width, board.Height);
            if (!occupancy.TryRebuildOccupancy(board.Vehicles.Values, board.Gates.Values, out _))
            {
                result = MoveResult.BlockedByVehicle;
                return false;
            }

            result = ValidateStep(
                board,
                occupancy,
                vehicleId,
                travelDirection,
                out candidateFootprint,
                out _,
                out _);

            if (result != MoveResult.ValidAdvance)
                return false;

            var vehicle = board.Vehicles[vehicleId];
            vehicle.HeadPosition = candidateFootprint[0];
            vehicle.Orientation = travelDirection;
            vehicle.CurrentVector = travelDirection;
            return true;
        }

        public static bool CanCompleteExit(
            BoardState board,
            int vehicleId,
            out List<GridPosition[]> stepFootprints,
            out MoveResult failureReason)
        {
            stepFootprints = new List<GridPosition[]>();
            failureReason = MoveResult.ValidExitCompleted;

            if (!board.Vehicles.TryGetValue(vehicleId, out var vehicle))
            {
                failureReason = MoveResult.VehicleNotFound;
                return false;
            }

            if (vehicle.IsExited)
            {
                failureReason = MoveResult.VehicleAlreadyExited;
                return false;
            }

            var simBoard = board.DeepClone();
            var occupancy = new OccupancySystem(simBoard.Width, simBoard.Height);

            if (!occupancy.TryRebuildOccupancy(
                    simBoard.Vehicles.Values,
                    simBoard.Gates.Values,
                    out _))
            {
                failureReason = MoveResult.BlockedByVehicle;
                return false;
            }

            var simVehicle = simBoard.Vehicles[vehicleId];

            // Phase 1: reach the exit portal using the current logical route.
            bool reachedExit = simBoard.Exits.ContainsKey(simVehicle.HeadPosition);
            int maxTravelSteps = simBoard.Width + simBoard.Height + simVehicle.Length + 8;
            int steps = 0;

            while (!reachedExit && steps++ < maxTravelSteps)
            {
                var stepResult = ValidateSingleStep(
                    simBoard,
                    occupancy,
                    simVehicle.Id,
                    out var candidate,
                    out _,
                    out _);

                if (stepResult != MoveResult.ValidAdvance)
                {
                    failureReason = stepResult;
                    return false;
                }

                stepFootprints.Add(candidate);
                simVehicle.HeadPosition = candidate[0];
                occupancy.TryRebuildOccupancy(
                    simBoard.Vehicles.Values,
                    simBoard.Gates.Values,
                    out _);

                reachedExit = simBoard.Exits.ContainsKey(simVehicle.HeadPosition);
            }

            if (!reachedExit)
            {
                failureReason = MoveResult.BlockedByBoundary;
                return false;
            }

            int destinationId = simBoard.Exits[simVehicle.HeadPosition];
            if (simVehicle.DestinationId != 0 &&
                destinationId != 0 &&
                simVehicle.DestinationId != destinationId)
            {
                failureReason = MoveResult.DestinationMismatch;
                return false;
            }

            // Phase 2: clear the complete footprint through the exit portal.
            // Every intermediate in-board body cell is still checked against
            // obstacles, gates and other vehicles.
            for (int k = 0; k < simVehicle.Length; k++)
            {
                var advancedHead = simVehicle.HeadPosition.Offset(simVehicle.CurrentVector, 1);
                var clearingFootprint = VehicleFootprint.CalculateOccupiedCells(
                    advancedHead,
                    simVehicle.Orientation,
                    simVehicle.Length);

                for (int i = 0; i < clearingFootprint.Length; i++)
                {
                    var cell = clearingFootprint[i];

                    // Cells outside the board are the vehicle leaving the puzzle.
                    if (!occupancy.IsInBounds(cell))
                        continue;

                    if (simBoard.Obstacles.Contains(cell))
                    {
                        failureReason = MoveResult.BlockedByObstacle;
                        return false;
                    }

                    if (simBoard.HasRoadNetwork &&
                        !simBoard.RoadGraph.IsConnected(cell))
                    {
                        failureReason = MoveResult.BlockedByRoad;
                        return false;
                    }

                    if (simBoard.Gates.TryGetValue(
                            GetGateIdAt(simBoard, cell),
                            out var gate) &&
                        gate.AllowedDirection != simVehicle.CurrentVector)
                    {
                        failureReason = MoveResult.BlockedByGate;
                        return false;
                    }

                    var occupant = occupancy.GetCell(cell).OccupyingVehicleId;
                    if (occupant.HasValue && occupant.Value != simVehicle.Id)
                    {
                        failureReason = MoveResult.BlockedByVehicle;
                        return false;
                    }
                }

                stepFootprints.Add(clearingFootprint);
                simVehicle.HeadPosition = advancedHead;
            }

            return true;
        }

        public static bool TryExecuteExit(
            BoardState board,
            int vehicleId,
            out List<GridPosition[]> trajectoryFootprints,
            out MoveResult result)
        {
            trajectoryFootprints = new List<GridPosition[]>();

            if (!CanCompleteExit(
                    board,
                    vehicleId,
                    out trajectoryFootprints,
                    out result))
            {
                return false;
            }

            board.Vehicles[vehicleId].IsExited = true;
            return true;
        }

        private static int GetGateIdAt(BoardState board, GridPosition position)
        {
            foreach (var gate in board.Gates.Values)
            {
                if (gate.Position == position)
                    return gate.Id;
            }

            return int.MinValue;
        }
    }
}
