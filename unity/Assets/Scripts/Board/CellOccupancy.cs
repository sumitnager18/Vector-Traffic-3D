using System;

namespace VectorTraffic3D.Board
{
    public enum TerrainType
    {
        Empty = 0,
        Road = 1,
        Junction = 2,
        Obstacle = 3,
        Exit = 4
    }

    /// <summary>
    /// Represents cell occupancy with strict layer precedence:
    /// 1. Vehicle (Dynamic blocker)
    /// 2. Gate (Directional constraint)
    /// 3. Exit (Egress portal)
    /// 4. Obstacle (Static blocker)
    /// 5. Road / Junction (Traversable surface)
    /// 6. Empty (Base surface)
    /// </summary>
    [Serializable]
    public struct CellOccupancy
    {
        public TerrainType Terrain;
        public int? OccupyingVehicleId;
        public int? GateId;
        public int? ExitDestinationId;

        public bool HasVehicle => OccupyingVehicleId.HasValue;
        public bool HasGate => GateId.HasValue;
        public bool IsExit => Terrain == TerrainType.Exit;
        public bool IsObstacle => Terrain == TerrainType.Obstacle;
        public bool IsRoad => Terrain == TerrainType.Road || Terrain == TerrainType.Junction;

        public static CellOccupancy DefaultEmpty => new CellOccupancy
        {
            Terrain = TerrainType.Empty,
            OccupyingVehicleId = null,
            GateId = null,
            ExitDestinationId = null
        };
    }
}
