using System;

namespace VectorTraffic3D.Core
{
    public enum Direction
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 3,
        Left = 4
    }

    public static class DirectionExtensions
    {
        public static Direction Opposite(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Right => Direction.Left,
                Direction.Left => Direction.Right,
                _ => Direction.None
            };
        }

        public static Direction RotateClockwise(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Right,
                Direction.Right => Direction.Down,
                Direction.Down => Direction.Left,
                Direction.Left => Direction.Up,
                _ => Direction.None
            };
        }

        public static Direction RotateCounterClockwise(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Left,
                Direction.Left => Direction.Down,
                Direction.Down => Direction.Right,
                Direction.Right => Direction.Up,
                _ => Direction.None
            };
        }

        public static (int dx, int dy) ToDelta(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => (0, 1),
                Direction.Right => (1, 0),
                Direction.Down => (0, -1),
                Direction.Left => (-1, 0),
                _ => (0, 0)
            };
        }
    }
}
