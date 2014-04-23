using System;
using System.Diagnostics;
using System.IO;
using AdvancedHookManaged;
using GTA;
using GTA.Native;
using SlimDX;

namespace ultimate_fuel_script
{
    /// <summary>
    /// Handles user input to action translations
    /// </summary>
    class controller : Script
    {
        #region Properties



        #endregion Properties

        public controller()
        {
            this.Interval = 250; // Run updates 4 times per second
            this.Tick += controller_Tick;
        }

        void controller_Tick(object sender, EventArgs e)
        {
            // Change CurrentFuelStation according to player location and moving method
            if (Player.Character.isInVehicle() && Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character && Player.Character.CurrentVehicle.Speed < 1.5f)
            {
                // This basically tests if the player is at the closest station found.
                model.CurrentFuelStation = FuelStation.IsAtStation(
                    FuelStation.GetNearestStation(
                        Player.Character.Position,
                        FuelStation.GetStationTypeFromVehicle(Player.Character.CurrentVehicle)),
                    Player.Character.Position);
            }
        }
    }
}
