using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTA;
using GTA.Native;
using AdvancedHookManaged;
using System.Drawing;

namespace ultimate_fuel_script
{
    class view : Script
    {
        /// <summary>
        /// The script entry point
        /// </summary>
        public view()
        {
            // Initialize the fuel gauge
            fuelGauge = new Gauge(new PointF(Settings.GetValueFloat("X", "DASHBOARD", 0.0f), Settings.GetValueFloat("Y", "DASHBOARD", 0.0f)), Settings.GetValueFloat("W", "DASHBOARD", 0.11f));


            // Set timer interval time
            this.Interval = 10;
            // Assign timer tick event
            this.Tick += new EventHandler(view_Tick);
            // Assign drawing frame event
            this.PerFrameDrawing += new GraphicsEventHandler(view_PerFrameDrawing);
        }

        #region Properties

        /// <summary>
        /// Fuel display gauge
        /// </summary>
        internal Gauge fuelGauge;
        /// <summary>
        /// Holds the fuel station the player is currently in, null if not at a fuel station or if any fuel station diferent than the vehicle type
        /// </summary>
        internal FuelStation currentFuelStation = null;

        #endregion Properties

        #region Events

        /// <summary>
        /// Main tick event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void view_Tick(object sender, EventArgs e)
        {
            // Display information to the player only when in a vehicle stoped at a fuel station
            if (Player.Character.isInVehicle() && Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character && Player.Character.CurrentVehicle.Speed < 1.5f)
            {
                // Get the station the player is at, if any
                currentFuelStation = FuelStation.IsAtStation(
                    FuelStation.GetNearestStation(
                        Player.Character.Position,
                        FuelStation.GetStationTypeFromVehicle(Player.Character.CurrentVehicle)),
                    Player.Character.Position);

                // Player is at a station, let's show some info
                if (currentFuelStation != null)
                {
                    // Display help message
                    if (currentFuelStation.DisplayBlip)
                        // Normal station message
                        DisplayHelp(String.Format("Welcome to ~y~{0}~w~. Hold ~INPUT_VEH_HANDBRAKE~ to refuel. ${1} per liter.",
                            currentFuelStation.Name,
                            currentFuelStation.Price));
                    else
                        // Hidden station message
                        DisplayHelp(String.Format("You found ~y~{0}~w~! Hold ~INPUT_VEH_HANDBRAKE~ to steal some fuel.",
                            currentFuelStation.Name));
                }
            }
        }
        /// <summary>
        /// Frame painting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void view_PerFrameDrawing(object sender, GraphicsEventArgs e)
        {
            // Gauge display
            try
            {
                // If player is in a vehicle and driving it, paint the gauge
                if (Player.Character.isInVehicle() && Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character)
                    fuelGauge.Draw(e.Graphics, Player.Character.CurrentVehicle);
            }
            catch (Exception E)
            {
                model.Log("PerFrameDrawing", E.Message);
            }
        }

        #endregion Events


        #region Methods

        /// <summary>
        /// Displays a message at the bottom of the screen
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="time">Time in milliseconds</param>
        internal static void DisplayInfo(string message, int time)
        {
            Function.Call("PRINT_STRING_WITH_LITERAL_STRING_NOW", "STRING", message, time, true);
        }
        /// <summary>
        /// Displays a help message at the top left corner of the screen
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="keep">Keep the message untill a new one</param>
        internal static void DisplayHelp(string message, bool keep = false)
        {
            if (keep)
                AGame.PrintText(message);
            else
                AGame.PrintTextForever(message);
        }
        /// <summary>
        /// Clears the latest help message
        /// </summary>
        internal static void ClearHelp()
        {
            Function.Call("CLEAR_HELP");
        }

        #endregion
    }
}
