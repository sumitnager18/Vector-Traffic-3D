namespace VectorTraffic3D.Vehicles
{
    public enum VehicleType
    {
        Hatchback = 0,       // 2 cells
        Sedan = 1,           // 3 cells
        SUV = 2,             // 3 cells
        Taxi = 3,            // 3 cells
        Pickup = 4,          // 3 cells
        Van = 5,             // 4 cells
        Bus = 6,             // 4 cells
        DeliveryTruck = 7    // 5 cells
    }

    public static class VehicleTypeExtensions
    {
        public static int GetDefaultLength(this VehicleType type)
        {
            return type switch
            {
                VehicleType.Hatchback => 2,
                VehicleType.Sedan => 3,
                VehicleType.SUV => 3,
                VehicleType.Taxi => 3,
                VehicleType.Pickup => 3,
                VehicleType.Van => 4,
                VehicleType.Bus => 4,
                VehicleType.DeliveryTruck => 5,
                _ => 2
            };
        }

        public static int GetDefaultWidth(this VehicleType type)
        {
            return 1;
        }
    }
}
