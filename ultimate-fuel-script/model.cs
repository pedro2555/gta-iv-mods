﻿using System;
using System.Diagnostics;
using System.IO;
using AdvancedHookManaged;
using GTA;
using GTA.Native;

namespace ultimate_fuel_script
{
    public enum Actions
    {
        None,
        Driving,
        Refueling
    }

    /// <summary>
    /// Handles core script functions and actions
    /// 
    /// Every action runs a on a per-tick notion, tick spacing is 250ms
    /// </summary>
    public class model : Script
    {
        #region Properties

        /// <summary>
        /// Fuel station that player is currently at, null if at any or not in a vehicle.
        /// </summary>
        public static FuelStation CurrentFuelStation
        { get; set; }

        public static FuelData CurrentFuelData
        { get; private set; }

        /// <summary>
        /// Keeps a record of the very last action
        /// </summary>
        public static Actions LastAction
        { get; private set; }
        /// <summary>
        /// Gets the current action
        /// </summary>
        public static Actions CurrentAction
        { get; private set; }

        internal static readonly string scriptName = "ultimate-fuel-script";
        #endregion Properties

        /// <summary>
        /// The script entry point
        /// </summary>
        public model()
        {
            // Tick spacing
            this.Interval = 250;
            // Tick handler
            this.Tick += new EventHandler(model_Tick);

            #region Log script start
            model.Log(" - - - - - - - - - - - - - - - STARTUP - - - - - - - - - - - - - - - ", String.Format("GTA IV {0} under {1}", Game.Version.ToString(), model.getOSInfo()));
            model.Log("Started", String.Format("{0} v{1}", model.scriptName, FileVersionInfo.GetVersionInfo(Game.InstallFolder + "\\scripts\\" + model.scriptName + ".net.dll").ProductVersion, true));
            #endregion Log script start
        }

        void model_Tick(object sender, EventArgs e)
        {
            // Ensure fuelData is always loaded
            if (Player.Character.isInVehicle())
            {
                if ((FuelData)Player.Character.CurrentVehicle.Metadata.Fuel == null)
                    Player.Character.CurrentVehicle.Metadata.Fuel = FuelData.Generate(Player.Character.CurrentVehicle, Settings);
                model.CurrentFuelData = (FuelData)Player.Character.CurrentVehicle.Metadata.Fuel;
            }

            switch (model.CurrentAction)
            {
                case Actions.Driving:
                    Player.Character.CurrentVehicle.Metadata.Fuel = FuelData.DrainFuel();
                    // Drain fuel
                    break;
                case Actions.Refueling:
                    // Refuel tank
                    break;
            }
        }

        #region Methods

        /// <summary>
        /// Sets LastAction as CurrentAction and updates CurrentAction with the specified action
        /// </summary>
        /// <param name="action"></param>
        public static void UpdateCurrentAction(Actions action)
        {
            model.LastAction = model.CurrentAction;
            model.CurrentAction = action;
        }

        /// <summary>
        /// Returns ini file entry
        /// </summary>
        /// <param name="Option"></param>
        /// <param name="Category"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public float GetValueFloat(string Option, string Category, float Default)
        {
            return Settings.GetValueFloat(Option, Category, Default);
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
                    using (StreamWriter streamWriter = File.AppendText(Game.InstallFolder + "\\scripts\\" + model.scriptName + ".log"))
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
