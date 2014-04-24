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

        private Gauge gauge
        { get; set; }

        /// <summary>
        /// Gets or sets if a welcome message has been displayed for the model.CurrentFuelStation
        /// </summary>
        public static bool StationWelcomeMessageHasBeenDisplayed
        { get; set; }

        #endregion Properties

        /// <summary>
        /// The script entry point
        /// </summary>
        public view()
        {
            // Load Gauge data
            gauge = new Gauge(
                new System.Drawing.PointF(Settings.GetValueFloat("X", "GAUGE", .09f), Settings.GetValueFloat("Y", "GAUGE", .96f)),
                Settings.GetValueFloat("W", "GAUGE", .11f));

            // Populate fuel stations on map
            FuelStation.LoadStations(Settings);

            this.Interval = 250; // Run updates 4 times a second
            this.Tick += view_Tick;

            // Handle graphis display
            this.PerFrameDrawing += view_PerFrameDrawing;
        }

        void view_PerFrameDrawing(object sender, GraphicsEventArgs e)
        {
            // Evertime player is driving a vehicle fuel data should be displayed.
            if (model.CurrentAction != Actions.None)
                // Display heads up stuff
                gauge.Draw(e.Graphics, model.CurrentFuelData);
        }

        void view_Tick(object sender, EventArgs e)
        {
            switch (model.CurrentAction)
            {
                case Actions.Driving:
                    if (model.LastAction == Actions.Refueling)
                    {
                        // Display refuel cost message
                        if (model.CurrentFuelStation.DisplayBlip)
                        {
                            DisplayHelp(string.Format("Thank you for refueling at ~y~{0}~w~, we loved your ${1:0.00}!", model.CurrentFuelStation.Name, 0f));
                            GTA.Native.Function.Call("DISPLAY_CASH");
                        }
                        else
                            DisplayHelp("Be on the look out for the ~b~cops~w~!");
                    }
                    else
                    {
                        if (model.CurrentFuelStation == null && view.StationWelcomeMessageHasBeenDisplayed)
                            view.StationWelcomeMessageHasBeenDisplayed = false;
                        else if (!view.StationWelcomeMessageHasBeenDisplayed)
                        {
                            if (model.CurrentFuelStation.DisplayBlip)
                                DisplayHelp(String.Format("Welcome to ~y~{0}~w~. Hold ~INPUT_VEH_HANDBRAKE~ to refuel. ${1} per liter.",
                                    model.CurrentFuelStation.Name,
                                    model.CurrentFuelStation.Price));
                            else
                                DisplayHelp(String.Format("You found ~y~{0}~w~! Hold ~INPUT_VEH_HANDBRAKE~ to steal some fuel.",
                                    model.CurrentFuelStation.Name));
                            view.StationWelcomeMessageHasBeenDisplayed = true;
                        }
                    }
                    break;
                case Actions.Refueling:
                    GTA.Native.Function.Call("PRINT_STRING_WITH_LITERAL_STRING_NOW", "STRING", "Refueling...", 500, 1);
                    // Display refuel message
                    break;
                default:
                    ClearHelp();
                    break;
            }
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
