using System;
using System.Diagnostics;
using System.IO;
using AdvancedHookManaged;
using GTA;
using GTA.Native;

namespace ultimate_overtaking_fix
{
    public class UltimateOvertakingFix : Script
    {
        #region Properties

        internal static readonly string scriptName = "ultimate-overtaking-fix";
        #endregion Properties

        /// <summary>
        /// The script entry point
        /// </summary>
        public UltimateOvertakingFix()
        {
            // Set timer interval time
            this.Interval = 100;
            // Assign timer tick event
            this.Tick += new EventHandler(MainScript_Tick);
            #region Log script start
            UltimateOvertakingFix.Log(" - - - - - - - - - - - - - - - STARTUP - - - - - - - - - - - - - - - ", String.Format("GTA IV {0} under {1}", Game.Version.ToString(), UltimateOvertakingFix.GetOSInfo()));
            UltimateOvertakingFix.Log("Started", String.Format("{0} v{1}", UltimateOvertakingFix.scriptName, FileVersionInfo.GetVersionInfo(Game.InstallFolder + "\\scripts\\" + UltimateOvertakingFix.scriptName + ".net.dll").ProductVersion, true));
            #endregion Log script start
        }

        /// <summary>
        /// Main tick event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainScript_Tick(object sender, EventArgs e)
        {
            try
            {
                // Player must be in a vehicle, in the driver seat, with the vehicle stopped
                if (Player.Character.isInVehicle() &&
                    Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character &&
                    Function.Call<bool>("IS_CAR_STOPPED", new Parameter[] { Player.Character.CurrentVehicle }))
                {
                    // Get a vehicle behind the layer vehicle in a 4 meter radius and order it's driver to stand still
                    var car = World.GetClosestVehicle(Player.Character.CurrentVehicle.GetOffsetPosition(new Vector3(0f, -5f, 0f)), 1f);
                    var carDriver = car.GetPedOnSeat(VehicleSeat.Driver);

                    carDriver.Task.StandStill(10);
                }
            }
            catch (Exception E)
            {
                Log("Tick", E.Message);
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
                    using (StreamWriter streamWriter = File.AppendText(Game.InstallFolder + "\\scripts\\" + UltimateOvertakingFix.scriptName + ".log"))
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
                    Game.Console.Print($"{message}: {2}");
            }

        }
        /// <summary>
        /// Get OS name and SP
        /// </summary>
        /// <returns></returns>
        internal static string GetOSInfo()
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
                operatingSystem += " " + GetOSArchitecture().ToString() + "-bit";
            }
            //Return the information we've gathered.
            return operatingSystem;
        }
        /// <summary>
        /// Get OS architecture
        /// </summary>
        /// <returns></returns>
        private static int GetOSArchitecture()
        {
            string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
        }
        #endregion Logging and Updating
        #endregion Methods
    }
}
