using NUnit.Framework;
using VectorTraffic3D.Board;
using VectorTraffic3D.Core;
using VectorTraffic3D.Vehicles;

namespace VectorTraffic3D.Tests
{
    [TestFixture]
    public class OccupancySystemTests
    {
        [Test]
        public void RebuildOccupancy_DetectsIllegalOverlap()
        {
            var occupancy = new OccupancySystem(8, 8);

            // Vehicle 1: Hatchback head at (3, 3) pointing Right -> cells (3, 3), (2, 3)
            var v1 = new VehicleState(1, VehicleType.Hatchback, new GridPosition(3, 3), Direction.Right, Direction.Right);

            // Vehicle 2: Sedan head at (2, 4) pointing Down -> cells (2, 4), (2, 3), (2, 2)
            // Note: cell (2, 3) overlaps with Vehicle 1!
            var v2 = new VehicleState(2, VehicleType.Sedan, new GridPosition(2, 4), Direction.Down, Direction.Down);

            bool success = occupancy.TryRebuildOccupancy(new[] { v1, v2 }, null, out string error);

            Assert.IsFalse(success);
            Assert.IsTrue(error.Contains("Illegal overlap"));
        }

        [Test]
        public void RebuildOccupancy_SucceedsForNonOverlappingVehicles()
        {
            var occupancy = new OccupancySystem(8, 8);

            var v1 = new VehicleState(1, VehicleType.Hatchback, new GridPosition(2, 2), Direction.Right, Direction.Right);
            var v2 = new VehicleState(2, VehicleType.Bus, new GridPosition(5, 5), Direction.Up, Direction.Up);

            bool success = occupancy.TryRebuildOccupancy(new[] { v1, v2 }, null, out string error);

            Assert.IsTrue(success);
            Assert.IsNull(error);
            Assert.AreEqual(1, occupancy.GetCell(new GridPosition(2, 2)).OccupyingVehicleId);
            Assert.AreEqual(1, occupancy.GetCell(new GridPosition(1, 2)).OccupyingVehicleId);
            Assert.AreEqual(2, occupancy.GetCell(new GridPosition(5, 5)).OccupyingVehicleId);
        }
    }
}
