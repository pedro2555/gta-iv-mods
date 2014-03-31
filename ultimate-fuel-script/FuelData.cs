using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ultimate_fuel_script
{
    public static class FuelData
    {
        /// <summary>
        /// Vehicle's current fuel level
        /// </summary>
        public static float Fuel
        { get; set; }
        /// <summary>
        /// Vehicle's tank capacity
        /// </summary>
        public static float Tank
        { get; set; }
        /// <summary>
        /// Vehicle's tank reserve level
        /// </summary>
        public static float Reserve
        { get; set; }
        /// <summary>
        /// Vehicle's drain constant
        /// </summary>
        public static float Drain
        { get; set; }

        public static void Update(GTA.Vehicle veh)
        {
            FuelData.Fuel = veh.Metadata.Fuel;
            FuelData.Tank = veh.Metadata.Tank;
            FuelData.Reserve = veh.Metadata.Reserve;
            FuelData.Drain = veh.Metadata.Drain;
        }
    }
}
