using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTA;

namespace ultimate_fuel_script
{
    /// <summary>
    /// Enumerations of vehicle types
    /// </summary>
    public enum VehicleTypes
    {
        CAR,
        BOAT,
        HELI
    }

    public static class VehicleType
    {
        /// <summary>
        /// Returns the appropriate VehicleTypes for a given vehicle
        /// </summary>
        /// <param name="veh"></param>
        /// <returns></returns>
        public static VehicleTypes GetVehicleTypeFromVehicle(Vehicle veh)
        {
            return (veh.Model.isHelicopter) ? VehicleTypes.HELI : (veh.Model.isBoat) ? VehicleTypes.BOAT : VehicleTypes.CAR;
        }
    }
}
