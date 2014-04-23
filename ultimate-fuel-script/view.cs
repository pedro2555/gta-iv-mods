using System;
using System.Diagnostics;
using System.IO;
using AdvancedHookManaged;
using GTA;
using GTA.Native;

namespace ultimate_fuel_script
{
    /// <summary>
    /// Handles information display
    /// </summary>
    public class view : Script
    {
        #region Properties



        #endregion Properties

        /// <summary>
        /// The script entry point
        /// </summary>
        public view()
        {
            // Populate fuel stations on map
            FuelStation.LoadStations(Settings);

            this.Tick += view_Tick;
        }

        void view_Tick(object sender, EventArgs e)
        {
            #region handle fuel station help message display

            if (model.CurrentFuelStation != null)
                if (model.CurrentFuelStation.DisplayBlip)
                {
                    // Normal station message

                    DisplayHelp(String.Format("Welcome to ~y~{0}~w~. Hold ~INPUT_VEH_HANDBRAKE~ to refuel. ${1} per liter.",
                        model.CurrentFuelStation.Name,
                        model.CurrentFuelStation.Price));
                }
                else
                {
                    // Hidden station message

                    DisplayHelp(String.Format("You found ~y~{0}~w~! Hold ~INPUT_VEH_HANDBRAKE~ to steal some fuel.",
                        model.CurrentFuelStation.Name));
                }

            #endregion handle fuel station help message display
        }


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

        #endregion Methods
    }
}
