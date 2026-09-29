using System;
using VectorTraffic3D.Core;

namespace VectorTraffic3D.Gates
{
    [Serializable]
    public class VectorGate : IEquatable<VectorGate>
    {
        public int Id { get; }
        public GridPosition Position { get; }
        public Direction AllowedDirection { get; set; }

        public VectorGate(int id, GridPosition position, Direction initialAllowedDirection)
        {
            Id = id;
            Position = position;
            AllowedDirection = initialAllowedDirection;
        }

        public void CycleDirection()
        {
            AllowedDirection = AllowedDirection.RotateClockwise();
        }

        public VectorGate Clone()
        {
            return new VectorGate(Id, Position, AllowedDirection);
        }

        public bool Equals(VectorGate other)
        {
            if (other is null) return false;
            return Id == other.Id && Position == other.Position && AllowedDirection == other.AllowedDirection;
        }

        public override bool Equals(object obj) => obj is VectorGate other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Id, Position, AllowedDirection);
    }
}
