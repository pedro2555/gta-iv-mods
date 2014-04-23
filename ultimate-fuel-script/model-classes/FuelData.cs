using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTA;

namespace ultimate_fuel_script
{
    /// <summary>
    /// Keeps track of the current vehicle fuel data
    /// </summary>
    public class FuelData
    {
        public FuelData()
        {
            this.Tank = 100f;
            this.Reserve = 10f;
            this.Drain = 10f;
            this.Fuel = 10f;
        }

        #region Properties

        /// <summary>
        /// Stores vehicle tank capacity
        /// </summary>
        public float Tank
        { get; set; }
        /// <summary>
        /// Stores vehicle reserve level point
        /// </summary>
        public float Reserve
        { get; set; }
        /// <summary>
        /// Stores vehicle drain constant
        /// </summary>
        public float Drain
        { get; set; }
        /// <summary>
        /// Stores vehicle current fuel value
        /// </summary>
        public float Fuel
        { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Generates a new instance of FuelData class to a given vehicle's metadata with data based on a given settings file
        /// </summary>
        /// <param name="veh"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static FuelData Generate(GTA.Vehicle veh, SettingsFile settings)
        {
            // Initialize FuelData object
            FuelData fuelData = new FuelData();
            // Defaults to 100
            fuelData.Tank = settings.GetValueFloat("TANK", veh.GetHashCode().ToString(), settings.GetValueFloat("TANK", veh.Name, 100));
            // Defaults to 10% of Tank
            fuelData.Reserve = settings.GetValueFloat("RESERVE", veh.GetHashCode().ToString(), settings.GetValueFloat("RESERVE", veh.Name, fuelData.Tank * .1f));
            // Defaults to ?somehing?
            fuelData.Drain = settings.GetValueFloat("DRAIN", veh.GetHashCode().ToString(), settings.GetValueFloat("DRAIN", veh.Name, 100));
            // Randomize fuel level
            fuelData.Fuel = new Random().Next((int)fuelData.Reserve + 1, (int)fuelData.Tank);

            model.Log("FuelData.Generate()", fuelData.Tank.ToString());

            // Return the attached data
            return fuelData;
        }

        #endregion

    }
}
