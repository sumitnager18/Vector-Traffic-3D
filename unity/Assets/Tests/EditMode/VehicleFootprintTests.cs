using NUnit.Framework;
using VectorTraffic3D.Core;
using VectorTraffic3D.Vehicles;

namespace VectorTraffic3D.Tests
{
    [TestFixture]
    public class VehicleFootprintTests
    {
        [Test]
        public void Hatchback_OccupiesTwoCells_InOppositeOfOrientation()
        {
            // Head at (3, 3), Orientation = Right (moves right), Body extends Left to (2, 3)
            var head = new GridPosition(3, 3);
            var cells = VehicleFootprint.CalculateOccupiedCells(head, Direction.Right, 2);

            Assert.AreEqual(2, cells.Length);
            Assert.AreEqual(new GridPosition(3, 3), cells[0]);
            Assert.AreEqual(new GridPosition(2, 3), cells[1]);
        }

        [Test]
        public void Bus_OccupiesFourCells_AlongVerticalAxis()
        {
            // Head at (2, 5), Orientation = Up, Body extends Down to (2, 4), (2, 3), (2, 2)
            var head = new GridPosition(2, 5);
            var cells = VehicleFootprint.CalculateOccupiedCells(head, Direction.Up, 4);

            Assert.AreEqual(4, cells.Length);
            Assert.AreEqual(new GridPosition(2, 5), cells[0]);
            Assert.AreEqual(new GridPosition(2, 4), cells[1]);
            Assert.AreEqual(new GridPosition(2, 3), cells[2]);
            Assert.AreEqual(new GridPosition(2, 2), cells[3]);
        }

        [Test]
        public void DeliveryTruck_OccupiesFiveCells()
        {
            var head = new GridPosition(6, 2);
            var cells = VehicleFootprint.CalculateOccupiedCells(head, Direction.Left, 5);

            Assert.AreEqual(5, cells.Length);
            Assert.AreEqual(new GridPosition(6, 2), cells[0]);
            Assert.AreEqual(new GridPosition(7, 2), cells[1]);
            Assert.AreEqual(new GridPosition(8, 2), cells[2]);
            Assert.AreEqual(new GridPosition(9, 2), cells[3]);
            Assert.AreEqual(new GridPosition(10, 2), cells[4]);
        }
    }
}
