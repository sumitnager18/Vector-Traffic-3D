using System;
using VectorTraffic3D.Core;

namespace VectorTraffic3D.Vehicles
{
    [Serializable]
    public class VehicleState : IEquatable<VehicleState>
    {
        public int Id { get; }
        public VehicleType Type { get; }
        public int Length { get; }
        public int Width { get; }
        public GridPosition HeadPosition { get; set; }
        public Direction Orientation { get; set; }
        public Direction CurrentVector { get; set; }
        public int DestinationId { get; }
        public bool IsExited { get; set; }

        public VehicleState(
            int id,
            VehicleType type,
            GridPosition headPosition,
            Direction orientation,
            Direction currentVector,
            int destinationId = 0,
            int? customLength = null)
        {
            Id = id;
            Type = type;
            Length = customLength ?? type.GetDefaultLength();
            Width = type.GetDefaultWidth();
            HeadPosition = headPosition;
            Orientation = orientation;
            CurrentVector = currentVector;
            DestinationId = destinationId;
            IsExited = false;
        }

        public GridPosition[] GetOccupiedCells()
        {
            if (IsExited) return Array.Empty<GridPosition>();
            return VehicleFootprint.CalculateOccupiedCells(HeadPosition, Orientation, Length);
        }

        public VehicleState Clone()
        {
            return new VehicleState(Id, Type, HeadPosition, Orientation, CurrentVector, DestinationId, Length)
            {
                IsExited = this.IsExited
            };
        }

        public bool Equals(VehicleState other)
        {
            if (other is null) return false;
            return Id == other.Id &&
                   HeadPosition == other.HeadPosition &&
                   Orientation == other.Orientation &&
                   CurrentVector == other.CurrentVector &&
                   IsExited == other.IsExited;
        }

        public override bool Equals(object obj) => obj is VehicleState other && Equals(other);

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, HeadPosition, Orientation, CurrentVector, IsExited);
        }
    }
}
