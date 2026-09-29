using System;
using System.Collections.Generic;
using VectorTraffic3D.Core;

namespace VectorTraffic3D.Vehicles
{
    /// <summary>
    /// Computes and verifies multi-cell footprints for vehicles based on head position, length, and orientation.
    /// In Vector Traffic 3D, vehicles occupy genuine multi-cell discrete grid positions.
    /// The head is at the leading edge; remaining body cells extend backward (opposite of orientation).
    /// </summary>
    public static class VehicleFootprint
    {
        public static GridPosition[] CalculateOccupiedCells(GridPosition headPosition, Direction orientation, int length)
        {
            if (length <= 0)
                throw new ArgumentException("Vehicle length must be at least 1 cell.", nameof(length));

            var cells = new GridPosition[length];
            var backwardDirection = orientation.Opposite();

            for (int i = 0; i < length; i++)
            {
                cells[i] = headPosition.Offset(backwardDirection, i);
            }

            return cells;
        }

        public static GridPosition GetTailPosition(GridPosition headPosition, Direction orientation, int length)
        {
            return headPosition.Offset(orientation.Opposite(), length - 1);
        }

        public static bool ContainsPosition(GridPosition headPosition, Direction orientation, int length, GridPosition target)
        {
            var cells = CalculateOccupiedCells(headPosition, orientation, length);
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == target) return true;
            }
            return false;
        }
    }
}
