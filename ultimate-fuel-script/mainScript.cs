﻿using System;
using System.Diagnostics;
using System.IO;
using AdvancedHookManaged;
using GTA;
using GTA.Native;
using System.Windows.Forms;

namespace ultimate_fuel_script
{
    public class mainScript : Script
    {
        #region Properties

        /// <summary>
        /// Holds the fuel station the player is currently in, null if not at a fuel station or if any fuel station diferent than the vehicle type
        /// </summary>
        internal FuelStation currentFuelStation = null;
        /// <summary>
        /// Defines if an help message should be displayed upon entering a station
        /// </summary>
        internal bool displayHelpMessage = true;

        internal static readonly string scriptName = "ultimate-fuel-script";
        #endregion Properties

        /// <summary>
        /// The script entry point
        /// </summary>
        public mainScript()
        {
            // Defauld script GUID
            GUID = new Guid("e027d24c-7db2-4ba7-924f-a4d72f5ae27e");

            // Set timer interval time
            this.Interval = 100;
            // Assign timer tick event
            this.Tick += new EventHandler(mainScript_Tick);
            #region Log script start
            mainScript.Log(" - - - - - - - - - - - - - - - STARTUP - - - - - - - - - - - - - - - ", String.Format("GTA IV {0} under {1}", Game.Version.ToString(), mainScript.getOSInfo()));
            mainScript.Log("Started", String.Format("{0} v{1}", mainScript.scriptName, FileVersionInfo.GetVersionInfo(Game.InstallFolder + "\\scripts\\" + mainScript.scriptName + ".net.dll").ProductVersion, true));
            #endregion Log script start
        }

        /// <summary>
        /// Main tick event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mainScript_Tick(object sender, EventArgs e)
        {
            ///
            /// TODO:
            ///

            #region Check player location

            try
            {
                if (Player.Character.isInVehicle())
                {
                    currentFuelStation = FuelStation.IsAtStation(FuelStation.GetNearestStation(Player.Character.Position, FuelStation.GetStationTypeFromVehicle(Player.Character.CurrentVehicle)), Player.Character.Position);
                    // Display an help message if necessary
                    if (currentFuelStation != null && displayHelpMessage)
                    {
                        // Display help message
                        if (currentFuelStation.DisplayBlip)
                        {
                            // Normal station message

                            DisplayHelp(String.Format("Welcome to ~y~{0}~w~. Hold ~ INPUT_VEH_HANDBRAKE ~ to refuel. ${1} per liter.",
                                currentFuelStation.Name,
                                currentFuelStation.Price));
                        }
                        else
                        {
                            // Hidden station message

                            DisplayHelp(String.Format("You found ~y~{0}~w~! Hold ~ INPUT_VEH_HANDBRAKE_ALT ~ to steal some fuel. ${1} per liter.",
                                currentFuelStation.Name,
                                currentFuelStation.Price));
                        }

                        displayHelpMessage = false;
                    }
                    else
                        displayHelpMessage = true;
                }
            }
            catch (Exception E)
            {
                Log("mainScript_Tick - check-player-location", E.Message);
            }

            #endregion Check player location

        }

        #region Methods

        ///
        /// TODO:
        ///

        /// <summary>
        /// Loads all types of stations
        /// </summary>
        private void LoadStations()
        {
            try
            {
                // Clear any existing records
                FuelStation.Items.Clear();
                // Load all types
                foreach (StationType type in (StationType[])Enum.GetValues(typeof(StationType)))
                    LoadStations(type);
            }
            catch (Exception E)
            {
                Log("LoadStations", E.Message);
            }
        }
        /// <summary>
        /// Loads a specific type of station
        /// </summary>
        /// <param name="type"></param>
        private void LoadStations(StationType type)
        {
            try
            {
                for (byte i = 1; i <= Byte.MaxValue; i++)
                {
                    // Get the station's location.
                    Vector3 stationLocation = Settings.GetValueVector3("LOCATION", type.ToString() + "STATION" + i,
                        new Vector3(-123456789.0987654321f, -123456789.0987654321f, -123456789.0987654321f));
                    if (stationLocation.X != -123456789.0987654321f && stationLocation.Y != -123456789.0987654321f && stationLocation.Z != -123456789.0987654321f)
                    {
                        FuelStation f = new FuelStation(
                            (Settings.GetValueString("NAME", type.ToString() + "STATION" + i, "Fuel Station").ToUpper().Trim().Length > 30) ?
                                Settings.GetValueString("NAME", type.ToString() + "STATION" + i, "Fuel Station").ToUpper().Trim().Substring(0, 29) :
                                Settings.GetValueString("NAME", type.ToString() + "STATION" + i, "Fuel Station").ToUpper().Trim(),
                            Settings.GetValueFloat("RADIUS", type.ToString() + "STATION" + i, 10.0f),
                            stationLocation,
                            Settings.GetValueInteger("STARS", type.ToString() + "STATION" + i, 0),
                            type,
                            Settings.GetValueBool("DISPLAY", type.ToString() + "STATION" + i, true),
                            Settings.GetValueFloat("PRICE", type.ToString() + "STATION" + i, 0.0f));
                        f.PlaceOnMap();
                        FuelStation.Items.Add(f);
                    }
                    else
                        break;
                }
            }
            catch (Exception E)
            {
                Log("LoadStations(Type)", E.Message);
            }
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
        #region Logging and Updating
        /// <summary>
        /// Append a new line to the log file
        /// </summary>
        /// <param name="methodName">The method that originated it</param>
        /// <param name="message">The exception's message</param>
        internal static void Log(string methodName, string message, bool printToConsole = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    using (StreamWriter streamWriter = File.AppendText(Game.InstallFolder + "\\scripts\\" + mainScript.scriptName + ".log"))
                    {
                        streamWriter.WriteLine("[{0}] @ {1}: {2}", DateTime.Now.ToString("hh:mm:ss.fff"), methodName, message);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
#if DEBUG
                    Game.DisplayText(String.Format("[{0}] @ {1}: {2}", DateTime.Now.ToString("hh:mm:ss.fff"), methodName, message), 1500);
#endif
                }
            }
            catch
            {

            }
            finally
            {
                if (printToConsole)
                    Game.Console.Print(String.Format("{1}: {2}", methodName, message));
            }

        }
        /// <summary>
        /// Get OS name and SP
        /// </summary>
        /// <returns></returns>
        internal static string getOSInfo()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = "2000";
                        else
                            operatingSystem = "XP";
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = "Vista";
                        else
                            operatingSystem = "7";
                        break;
                    default:
                        break;
                }
            }
            //Make sure we actually got something in our OS check
            //We don't want to just return " Service Pack 2" or " 32-bit"
            //That information is useless without the OS version.
            if (operatingSystem != "")
            {
                //Got something.  Let's prepend "Windows" and get more info.
                operatingSystem = "Windows " + operatingSystem;
                //See if there's a service pack installed.
                if (os.ServicePack != "")
                {
                    //Append it to the OS name.  i.e. "Windows XP Service Pack 3"
                    operatingSystem += " " + os.ServicePack;
                }
                //Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
                operatingSystem += " " + getOSArchitecture().ToString() + "-bit";
            }
            //Return the information we've gathered.
            return operatingSystem;
        }
        /// <summary>
        /// Get OS architecture
        /// </summary>
        /// <returns></returns>
        private static int getOSArchitecture()
        {
            string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
        }
        #endregion Logging and Updating
        #endregion Methods
    }
}
