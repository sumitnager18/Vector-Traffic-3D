using System;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Puzzle;

namespace VectorTraffic3D.Generation
{
    /// <summary>
    /// Conservative reverse-transition helper. It never invents a predecessor by simply
    /// changing coordinates; it reconstructs a candidate predecessor and replays the
    /// authoritative forward transition to prove that the candidate reaches the target.
    /// This makes it safe to use as the basis for future backwards level generation.
    /// </summary>
    public static class ReverseTransitionModel
    {
        public static bool TryUndoMovement(
            BoardState target,
            int vehicleId,
            Direction travelDirection,
            Direction previousOrientation,
            out BoardState predecessor)
        {
            predecessor = null;

            if (target == null || !target.Vehicles.TryGetValue(vehicleId, out var targetVehicle))
                return false;
            if (targetVehicle.IsExited || target.Exits.ContainsKey(targetVehicle.HeadPosition))
                return false;
            if (travelDirection == Direction.None)
                return false;

            var candidate = target.DeepClone();
            var vehicle = candidate.Vehicles[vehicleId];

            // Forward MoveValidator sets both orientation and vector to travelDirection.
            // Therefore the target state must agree with the proposed forward action.
            if (vehicle.Orientation != travelDirection || vehicle.CurrentVector != travelDirection)
                return false;

            vehicle.HeadPosition = vehicle.HeadPosition.Offset(travelDirection.Opposite());
            vehicle.Orientation = previousOrientation;
            vehicle.CurrentVector = previousOrientation;

            if (!candidate.TryValidateStaticState(out _))
                return false;

            var replay = candidate.DeepClone();
            if (!MoveValidator.TryExecuteStep(replay, vehicleId, travelDirection, out _, out _))
                return false;

            if (!StatesMatchExceptVehiclePositionReplay(target, replay, vehicleId))
                return false;

            predecessor = candidate;
            return true;
        }

        private static bool StatesMatchExceptVehiclePositionReplay(
            BoardState expected,
            BoardState actual,
            int vehicleId)
        {
            foreach (var pair in expected.Vehicles)
            {
                if (!actual.Vehicles.TryGetValue(pair.Key, out var actualVehicle))
                    return false;

                var expectedVehicle = pair.Value;
                if (pair.Key == vehicleId)
                {
                    if (actualVehicle.HeadPosition != expectedVehicle.HeadPosition ||
                        actualVehicle.Orientation != expectedVehicle.Orientation ||
                        actualVehicle.CurrentVector != expectedVehicle.CurrentVector ||
                        actualVehicle.IsExited != expectedVehicle.IsExited)
                        return false;
                }
                else if (!actualVehicle.Equals(expectedVehicle))
                {
                    return false;
                }
            }

            foreach (var pair in expected.Gates)
            {
                if (!actual.Gates.TryGetValue(pair.Key, out var gate) || !gate.Equals(pair.Value))
                    return false;
            }

            return true;
        }
    }
}
