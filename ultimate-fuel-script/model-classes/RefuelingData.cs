using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ultimate_fuel_script
{
    public static class RefuelingData
    {
        #region Properties
        /// <summary>
        /// Cost per liter reported by the fuel station
        /// </summary>
        public static float LiterCost
        { get; set; }
        /// <summary>
        /// Total in liter refueled
        /// </summary>
        public static float LiterConsumed
        { get; set; }
        /// <summary>
        /// Cost of refueling
        /// </summary>
        public static float TotalCost
        {
            get
            {
                return LiterConsumed * LiterCost;
            }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Reset all data to it's defaults
        /// </summary>
        public static void Reset()
        {
            LiterCost = 0.0f;
            LiterConsumed = 0.0f;
        }

        /// <summary>
        /// Tests if player can keep refueling
        /// </summary>
        /// <param name="Money"></param>
        /// <returns></returns>
        public static bool CanRefuel(float Money)
        {
            return TotalCost >= Money;
        }

        #endregion Methods
    }
}
