using System;
using System.Diagnostics;
using System.IO;
using AdvancedHookManaged;
using GTA;
using GTA.Native;
using System.Media;

namespace ultimate_fuel_script
{
    /// <summary>
    /// Handles information display
    /// </summary>
    public class view : Script
    {
        #region Properties

        /// <summary>
        /// Ultimate Indicator Script Guid
        /// </summary>
        public readonly Guid UltimateIndicatorScript = new Guid("775df3cb-41c0-45f7-bd8f-d989853c838b");
        /// <summary>
        /// Graphical fuel level displayer
        /// </summary>
        private Gauge gauge
        { get; set; }
        /// <summary>
        /// Gets or sets if a welcome message has been displayed for the model.CurrentFuelStation
        /// </summary>
        public static bool StationWelcomeMessageHasBeenDisplayed
        { get; set; }
        /// <summary>
        /// Gets or sets if the reserve level beep indicator has already played
        /// </summary>
        public static bool ReserveLevelNotifications
        { get; set; }

        #endregion Properties

        /// <summary>
        /// The script entry point
        /// </summary>
        public view()
        {
            // Initiate working propeties
            view.StationWelcomeMessageHasBeenDisplayed = false;
            view.ReserveLevelNotifications = false;

            // Load Gauge data
            gauge = new Gauge(
                new System.Drawing.PointF(Settings.GetValueFloat("X", "GAUGE", .09f), Settings.GetValueFloat("Y", "GAUGE", .96f)),
                Settings.GetValueFloat("W", "GAUGE", .11f));

            // Populate fuel stations on map
            FuelStation.LoadStations(Settings);

            this.Interval = 50; // Run updates 4 times a second
            this.Tick += view_Tick;

            // Handle graphis display
            this.PerFrameDrawing += view_PerFrameDrawing;
        }

        void view_PerFrameDrawing(object sender, GraphicsEventArgs e)
        {
            // Evertime player is driving a vehicle, fuel data should be displayed.
            if (model.CurrentAction != Actions.None)
                // Display heads up stuff
                gauge.Draw(e.Graphics, model.CurrentFuelData);
        }

        void view_Tick(object sender, EventArgs e)
        {
            try {
                if (model.CurrentFuelData != null)
                    switch (model.CurrentAction)
                    {
                        case Actions.Driving:
                            // Handle reserve sound
                            if (model.CurrentFuelData.isOnReserve && !view.ReserveLevelNotifications && Player.Character.CurrentVehicle.Speed > 5f)
                            {
                                view.Play("reserve");
                                view.ReserveLevelNotifications = true;
                                if (model.CurrentFuelData.isEmpty && isScriptRunning(UltimateIndicatorScript))
                                        SendScriptCommand(UltimateIndicatorScript, "HazardsOn", Player.Character.CurrentVehicle, true);
                            }
                            if (model.LastAction == Actions.Refueling)
                            {
                                if (isScriptRunning(UltimateIndicatorScript))
                                    SendScriptCommand(UltimateIndicatorScript, "ResetAll", Player.Character.CurrentVehicle);
                                // Display refuel cost message
                                if (model.CurrentFuelStation.DisplayBlip)
                                {
                                    DisplayHelp(string.Format("Thank you for refueling at ~y~{0}~w~, we loved your ~g~${1:0}~w~!", model.CurrentFuelStation.Name, model.CurrentRefuelData.TotalCostRounded));
                                    GTA.Native.Function.Call("DISPLAY_CASH");
                                }
                                else
                                {
                                    DisplayHelp("Be on the look out for the ~b~cops~w~!");
                                    // Give the specified amount of stars to the player
                                    Player.WantedLevel = (Player.WantedLevel < model.CurrentFuelStation.WantedStars) ? model.CurrentFuelStation.WantedStars : Player.WantedLevel;
                                }
                            }
                            else
                            {
                                // Handle stations specific messages
                                if (model.CurrentFuelStation == null && view.StationWelcomeMessageHasBeenDisplayed)
                                    view.StationWelcomeMessageHasBeenDisplayed = false;
                                else if (!view.StationWelcomeMessageHasBeenDisplayed && model.CurrentFuelStation != null)
                                {
                                    if (model.CurrentFuelStation.DisplayBlip)
                                        DisplayHelp(String.Format("Welcome to ~y~{0}~w~. Hold ~{1}~ to refuel. ~g~${2}~w~ per liter.",
                                            model.CurrentFuelStation.Name,
                                            controller.RefuelInputSet.InputSetName,
                                            model.CurrentFuelStation.Price));
                                    else
                                        DisplayHelp(String.Format("You found ~y~{0}~w~! Hold ~{1}~ to steal some fuel.",
                                            model.CurrentFuelStation.Name,
                                            controller.RefuelInputSet.InputSetName));
                                    view.StationWelcomeMessageHasBeenDisplayed = true;
                                }
                            }
                            if (view.ReserveLevelNotifications && !model.CurrentFuelData.isOnReserve)
                            {
                                // Request a indicator script unlock
                                SendScriptCommand(UltimateIndicatorScript, "ResetAll", Player.Character.CurrentVehicle);
                                view.ReserveLevelNotifications = false;
                            }
                            break;
                        case Actions.Refueling:
                            GTA.Native.Function.Call("PRINT_STRING_WITH_LITERAL_STRING_NOW", "STRING", String.Format("Refueling . . . ~n~~b~{0} liters ~w~for ~g~${1}~w~", model.CurrentRefuelData.UnitCount.ToString("F2"), model.CurrentRefuelData.TotalCostRounded), 500, 1);
                            break;
                        case Actions.None:
                            view.ReserveLevelNotifications = false;
                            break;
                        default:
                            ClearHelp();
                            break;
                    }
            
            }
            catch (Exception E)
            {
                model.Log("view_tick", E.Message, true);
            }
        }

        #region Methods

        /// <summary>
        /// Play a specific sound from the embedded resources
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Play(string sound)
        {
            try
            {
                // Play the requested sound
                new SoundPlayer(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ultimate_fuel_script.resources." + sound + ".wav")).Play();
            }
            catch (Exception crap) { model.Log("Play", crap.Message); }
        }
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