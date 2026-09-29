using System;
using System.Collections.Generic;
using System.Linq;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Gates;
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
        DestinationMismatch,
        VehicleAlreadyExited,
        VehicleNotFound
    }

    /// <summary>
    /// Authoritative movement and exit validation for Vector Traffic 3D.
    /// Operates strictly on the COMPLETE multi-cell vehicle footprint at every step.
    /// Never checks only the head cell.
    /// </summary>
    public static class MoveValidator
    {
        /// <summary>
        /// Validates whether a vehicle can advance a single discrete step forward along its vector.
        /// Inspects EVERY cell of the candidate footprint against boundaries, obstacles, gates, and other vehicles.
        /// </summary>
        public static MoveResult ValidateSingleStep(
            BoardState board,
            OccupancySystem occupancy,
            int vehicleId,
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

            var travelVector = vehicle.CurrentVector;
            var newHeadPos = vehicle.HeadPosition.Offset(travelVector, 1);

            // Compute candidate footprint for entire vehicle body
            candidateFootprint = VehicleFootprint.CalculateOccupiedCells(newHeadPos, vehicle.Orientation, vehicle.Length);

            // Check if head is entering an exit
            bool headAtExit = board.Exits.TryGetValue(newHeadPos, out int exitDestId);

            // Validate EVERY occupied cell in candidate footprint
            for (int i = 0; i < candidateFootprint.Length; i++)
            {
                var cell = candidateFootprint[i];

                // If this is the head cell and it reached an exit
                if (i == 0 && headAtExit)
                {
                    if (vehicle.DestinationId != 0 && exitDestId != 0 && vehicle.DestinationId != exitDestId)
                    {
                        blockedCellIndex = 0;
                        blockedCellPos = cell;
                        return MoveResult.DestinationMismatch;
                    }
                    // Head at valid exit portal; continue checking remaining trailing cells
                    continue;
                }

                // 1. Boundary check on this footprint cell
                if (!occupancy.IsInBounds(cell))
                {
                    blockedCellIndex = i;
                    blockedCellPos = cell;
                    return MoveResult.BlockedByBoundary;
                }

                // 2. Static Obstacle check
                if (board.Obstacles.Contains(cell))
                {
                    blockedCellIndex = i;
                    blockedCellPos = cell;
                    return MoveResult.BlockedByObstacle;
                }

                // 3. Vector Gate check
                foreach (var gate in board.Gates.Values)
                {
                    if (gate.Position == cell)
                    {
                        if (gate.AllowedDirection != travelVector)
                        {
                            blockedCellIndex = i;
                            blockedCellPos = cell;
                            return MoveResult.BlockedByGate;
                        }
                    }
                }

                // 4. Vehicle collision check:
                // Collides if occupied by ANOTHER vehicle (not by this vehicle's own current cells)
                var currentOccupant = occupancy.GetCell(cell).OccupyingVehicleId;
                if (currentOccupant.HasValue && currentOccupant.Value != vehicle.Id)
                {
                    blockedCellIndex = i;
                    blockedCellPos = cell;
                    return MoveResult.BlockedByVehicle;
                }
            }

            return MoveResult.ValidAdvance;
        }

        /// <summary>
        /// Validates whether a vehicle has a completely clear, unobstructed route to travel
        /// and fully exit the board.
        /// A vehicle is NOT considered exited merely because its head reaches an exit cell;
        /// the entire footprint of length L must clear through the exit portal without any collision
        /// at any intermediate step.
        /// </summary>
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

            // Create working simulator copy of board to advance step-by-step
            var simBoard = board.DeepClone();
            var occupancy = new OccupancySystem(simBoard.Width, simBoard.Height);
            if (!occupancy.TryRebuildOccupancy(simBoard.Vehicles.Values, simBoard.Gates.Values, out _))
            {
                failureReason = MoveResult.BlockedByVehicle;
                return false;
            }

            var simVehicle = simBoard.Vehicles[vehicleId];
            int maxTravelSteps = simBoard.Width + simBoard.Height + simVehicle.Length + 4;
            int steps = 0;
            bool reachedExit = false;
            GridPosition exitPos = default;
            int exitDestId = 0;

            // Phase 1: Advance towards and reach the exit portal
            while (steps++ < maxTravelSteps && !reachedExit)
            {
                var nextHead = simVehicle.HeadPosition.Offset(simVehicle.CurrentVector, 1);

                if (simBoard.Exits.TryGetValue(nextHead, out exitDestId))
                {
                    if (simVehicle.DestinationId != 0 && exitDestId != 0 && simVehicle.DestinationId != exitDestId)
                    {
                        failureReason = MoveResult.DestinationMismatch;
                        return false;
                    }

                    // Check if head entering exit is clear
                    var candidateFootprint = VehicleFootprint.CalculateOccupiedCells(nextHead, simVehicle.Orientation, simVehicle.Length);
                    for (int i = 0; i < candidateFootprint.Length; i++)
                    {
                        var cell = candidateFootprint[i];
                        if (i == 0) continue; // Head is at exit portal

                        if (!occupancy.IsInBounds(cell))
                        {
                            failureReason = MoveResult.BlockedByBoundary;
                            return false;
                        }
                        if (simBoard.Obstacles.Contains(cell))
                        {
                            failureReason = MoveResult.BlockedByObstacle;
                            return false;
                        }
                        var occ = occupancy.GetCell(cell).OccupyingVehicleId;
                        if (occ.HasValue && occ.Value != simVehicle.Id)
                        {
                            failureReason = MoveResult.BlockedByVehicle;
                            return false;
                        }
                    }

                    stepFootprints.Add(candidateFootprint);
                    simVehicle.HeadPosition = nextHead;
                    exitPos = nextHead;
                    reachedExit = true;
                    break;
                }

                // Normal advance step
                var stepResult = ValidateSingleStep(simBoard, occupancy, simVehicle.Id, out var candidate, out _, out _);
                if (stepResult != MoveResult.ValidAdvance)
                {
                    failureReason = stepResult;
                    return false;
                }

                stepFootprints.Add(candidate);
                simVehicle.HeadPosition = candidate[0];
                occupancy.TryRebuildOccupancy(simBoard.Vehicles.Values, simBoard.Gates.Values, out _);
            }

            if (!reachedExit)
            {
                failureReason = MoveResult.BlockedByBoundary;
                return false;
            }

            // Phase 2: Complete exit clearance
            // For a vehicle of length L, its trailing L - 1 cells must clear through the exit portal.
            // At each clearing step k from 1 to L:
            for (int k = 1; k < simVehicle.Length; k++)
            {
                var advancedHead = simVehicle.HeadPosition.Offset(simVehicle.CurrentVector, 1);
                var clearingFootprint = VehicleFootprint.CalculateOccupiedCells(advancedHead, simVehicle.Orientation, simVehicle.Length);

                // Any trailing cell still on the board must not collide with other vehicles or obstacles
                for (int i = k; i < clearingFootprint.Length; i++)
                {
                    var cell = clearingFootprint[i];
                    if (occupancy.IsInBounds(cell))
                    {
                        if (simBoard.Obstacles.Contains(cell))
                        {
                            failureReason = MoveResult.BlockedByObstacle;
                            return false;
                        }
                        var occ = occupancy.GetCell(cell).OccupyingVehicleId;
                        if (occ.HasValue && occ.Value != simVehicle.Id)
                        {
                            failureReason = MoveResult.BlockedByVehicle;
                            return false;
                        }
                    }
                }

                stepFootprints.Add(clearingFootprint);
                simVehicle.HeadPosition = advancedHead;
            }

            failureReason = MoveResult.ValidExitCompleted;
            return true;
        }

        /// <summary>
        /// Authoritatively executes an exit transition if legal.
        /// Updates board state: marks vehicle as exited and clears dynamic occupancy.
        /// </summary>
        public static bool TryExecuteExit(
            BoardState board,
            int vehicleId,
            out List<GridPosition[]> trajectoryFootprints,
            out MoveResult result)
        {
            trajectoryFootprints = new List<GridPosition[]>();

            if (!CanCompleteExit(board, vehicleId, out trajectoryFootprints, out result))
            {
                return false;
            }

            // Commit logical transition
            board.Vehicles[vehicleId].IsExited = true;
            return true;
        }
    }
}
