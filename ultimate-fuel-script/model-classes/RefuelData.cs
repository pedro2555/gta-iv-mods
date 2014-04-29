using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTA;

namespace ultimate_fuel_script
{
    public class RefuelData
    {
        public RefuelData(FuelStation station)
        {
            this.UnitCost = station.Price;
            this.UnitCount = 0.0f;
        }

        #region Properties

        /// <summary>
        /// Cost per fuel unit
        /// </summary>
        public float UnitCost
        { get; set; }
        /// <summary>
        /// Total units refueled
        /// </summary>
        public float UnitCount
        { get; set; }
        /// <summary>
        /// Total exact cost
        /// </summary>
        public float TotalCost
        {
            get
            {
                return UnitCost * UnitCount;
            }
        }
        /// <summary>
        /// Total cost rounded
        /// </summary>
        public int TotalCostRounded
        {
            get
            {
                return Convert.ToInt32(TotalCost);
            }
        }
        /// <summary>
        /// Refuel tick for cars
        /// </summary>
        private static float CarRefuelTick
        { get; set; }
        /// <summary>
        /// Refuel tick for boats
        /// </summary>
        private static float BoatRefuelTick
        { get; set; }
        /// <summary>
        /// Refuel tick for helicopters
        /// </summary>
        private static float HeliRefuelTick
        { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Increments UnitCount, increments by 1 if no value is specified.
        /// </summary>
        /// <param name="amountToAdd"></param>
        public void UpdateUnitCount(float amountToAdd = 1.0f)
        {
            this.UnitCount += amountToAdd;
        }
        /// <summary>
        /// Returns true if player has enought money to continue refueling, false if it doesn't
        /// </summary>
        /// <param name="Money"></param>
        /// <returns></returns>
        public bool CanRefuel(float Money, Vehicle veh, FuelStation station)
        {
            switch (VehicleType.GetVehicleTypeFromVehicle(veh))
            {
                case VehicleTypes.CAR:
                    return (station.Price * FuelStation.CarRefuelTick) + this.TotalCost <= Money;
                case VehicleTypes.BOAT:
                    return (station.Price * FuelStation.BoatRefuelTick) + this.TotalCost <= Money;
                case VehicleTypes.HELI:
                    return (station.Price * FuelStation.HeliRefuelTick) + this.TotalCost <= Money;
                default:
                    return false;
            }

        }

        /// <summary>
        /// Reads refuel tick data from ini file
        /// </summary>
        /// <param name="settings"></param>
        public static void InitializeRefuelTick(SettingsFile settings)
        {
            CarRefuelTick = settings.GetValueFloat("CARREFUELTICK", .25f);
            BoatRefuelTick = settings.GetValueFloat("BOATREFUELTICK", .5f);
            HeliRefuelTick = settings.GetValueFloat("HELIREFUELTICK", 1.0f);
        }
        /// <summary>
        /// Returns refuel tick for a specific vehicle
        /// </summary>
        /// <param name="veh"></param>
        /// <returns></returns>
        public static float GetRefuelTick(Vehicle veh)
        {
            return GetRefuelTick(VehicleType.GetVehicleTypeFromVehicle(veh));
        }
        /// <summary>
        /// Returns refuel tick for a specific vehicle type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static float GetRefuelTick(VehicleTypes type)
        {
            switch (type)
            {
                default:
                case VehicleTypes.CAR:
                    return CarRefuelTick;
                case VehicleTypes.BOAT:
                    return BoatRefuelTick;
                case VehicleTypes.HELI:
                    return HeliRefuelTick;
            }
        }

        #endregion Methods
    }
}
