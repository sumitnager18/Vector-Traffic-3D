using UnityEngine;
using VectorTraffic3D.Core;
namespace VectorTraffic3D.Runtime
{
    public sealed class GridWorldMapper
    {
        public float CellSize { get; }
        public Vector3 Origin { get; }
        public GridWorldMapper(float cellSize = 1.5f, Vector3 origin = default)
        {
            CellSize = Mathf.Max(.25f, cellSize);
            Origin = origin;
        }
        public Vector3 ToWorld(GridPosition p, float y = 0f) =>
            Origin + new Vector3(p.X * CellSize, y, p.Y * CellSize);
        public Vector3 DirectionToWorld(Direction d) => d switch
        {
            Direction.Up => Vector3.forward,
            Direction.Right => Vector3.right,
            Direction.Down => Vector3.back,
            Direction.Left => Vector3.left,
            _ => Vector3.zero
        };
    }
}
