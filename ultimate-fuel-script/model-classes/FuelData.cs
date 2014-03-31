using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ultimate_fuel_script.model_classes
{
    /// <summary>
    /// Keeps track of the current vehicle fuel data
    /// </summary>
    public static class FuelData
    {
        #region Properties

        /// <summary>
        /// Stores vehicle tank capacity
        /// </summary>
        public static float Tank
        { get; set; }
        /// <summary>
        /// Stores vehicle reserve level point
        /// </summary>
        public static float Reserve
        { get; set; }
        /// <summary>
        /// Stores vehicle drain constant
        /// </summary>
        public static float Drain
        { get; set; }
        /// <summary>
        /// Stores vehicle current fuel value
        /// </summary>
        public static float Fuel
        { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Update FuelData objects with data from vehicle metada
        /// </summary>
        /// <param name="Metadata"></param>
        public void Update(ref GTA.Vehicle veh)
        {

        }

        #endregion

    }
}
