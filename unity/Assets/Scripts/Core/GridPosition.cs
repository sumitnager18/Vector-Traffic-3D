using System;

namespace VectorTraffic3D.Core
{
    [Serializable]
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public readonly int X;
        public readonly int Y;

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public GridPosition Add(int dx, int dy) => new GridPosition(X + dx, Y + dy);
        public GridPosition Offset(Direction direction, int steps = 1)
        {
            return direction switch
            {
                Direction.Up => new GridPosition(X, Y + steps),
                Direction.Right => new GridPosition(X + steps, Y),
                Direction.Down => new GridPosition(X, Y - steps),
                Direction.Left => new GridPosition(X - steps, Y),
                _ => this
            };
        }

        public int ManhattanDistance(GridPosition other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

        public bool Equals(GridPosition other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";

        public static bool operator ==(GridPosition left, GridPosition right) => left.Equals(right);
        public static bool operator !=(GridPosition left, GridPosition right) => !left.Equals(right);
    }
}
