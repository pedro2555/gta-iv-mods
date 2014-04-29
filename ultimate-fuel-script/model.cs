using System;
using System.Diagnostics;
using System.IO;
using AdvancedHookManaged;
using GTA;
using GTA.Native;
using System.Collections.Generic;

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

        public static Script script
        { get; private set; }
        /// <summary>
        /// List of script GUIDs subscribed to FuelData changes
        /// </summary>
        public static List<Guid> FuelDataSubscribers
        { get; set; }
        /// <summary>
        /// Fuel station that player is currently at, null if at any or not in a vehicle.
        /// </summary>
        public static FuelStation CurrentFuelStation
        { get; set; }
        /// <summary>
        /// Returns the latest used FuelData object
        /// </summary>
        public static FuelData CurrentFuelData
        { get; private set; }
        /// <summary>
        /// Holds the last or current refuel amount in liters
        /// </summary>
        public static RefuelData CurrentRefuelData
        { get; set; }
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
            this.GUID = new Guid("775df3cb-41c0-45f7-bd8f-d989853c838b");
            // Sends a one time fuel data object
            BindScriptCommand("FuelDataRequest", new ScriptCommandDelegate(FuelDataRequest_handler));
            // Subscribes the sender to continuos notification of fuel data changes
            BindScriptCommand("FuelDataSubscription", new ScriptCommandDelegate(FuelDataSubscription_handler));

            // Initialize refuel data
            RefuelData.InitializeRefuelTick(this.Settings);

            // Tick spacing
            this.Interval = 100;
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
                // Check for existing fuel data
                if ((FuelData)Player.Character.CurrentVehicle.Metadata.Fuel == null)
                    // Populate random fuel if none has been found
                    Player.Character.CurrentVehicle.Metadata.Fuel = FuelData.Generate(Player.Character.CurrentVehicle, Settings);
                // Update cross script fuel data
                UpdateFuelData((FuelData)Player.Character.CurrentVehicle.Metadata.Fuel);
            }
            else
                // Player is not in vehicle set cross script fuel data to null
                UpdateFuelData(null);

            switch (model.CurrentAction)
            {
                case Actions.Driving:
                    if (Player.Character.isInVehicle())
                    {
                        // Drain fuel
                        Player.Character.CurrentVehicle.Metadata.Fuel.DrainFuel(true, true, true, Player.Character.CurrentVehicle);
                        // Update cross script data
                        UpdateFuelData((FuelData)Player.Character.CurrentVehicle.Metadata.Fuel);
                        // Force vehicle to stop when without fuel
                        if (Player.Character.CurrentVehicle.Metadata.Fuel.Fuel <= 0.0f)
                            Player.Character.CurrentVehicle.EngineRunning = false;
                        // Only force engine on when the last action was a refuel action
                        else if (model.LastAction == Actions.Refueling)
                        {
                            Player.Character.CurrentVehicle.EngineRunning = true;

                            Player.Money -= (int)model.CurrentRefuelData.TotalCostRounded;
                            model.CurrentRefuelData.UnitCount = 0;
                        }
                    }
                    break;
                case Actions.Refueling:
                    // if just started refueling set LastRefuel data to 0.0f
                    if (model.LastAction == Actions.Driving)
                        model.CurrentRefuelData = new RefuelData(model.CurrentFuelStation);
                    // Stop the car
                    Player.Character.CurrentVehicle.EngineRunning = false;

                    // Actually refuel
                    model.CurrentRefuelData.UpdateUnitCount(Player.Character.CurrentVehicle.Metadata.Fuel.AddFuel(FuelStation.GetRefuelTick(VehicleType.GetVehicleTypeFromVehicle(Player.Character.CurrentVehicle))));

                    // Update cross script data
                    UpdateFuelData((FuelData)Player.Character.CurrentVehicle.Metadata.Fuel);
                    break;
                case Actions.None:
                    view.ReserveBeepHasBeenPlayed = false;
                    break;
            }
        }

        #region Methods

        #region Script Commands

        /// <summary>
        /// Handler for the FuelDataRequest Script Command
        /// 
        /// This command returns the current instance of the FuelData class from the vehicle the player is driving
        /// 
        /// Bind to FuelDataResponse to obtain a response to this request
        /// Returns null if the player is not in a vehicle or if the fuel data hasn't yet been loaded
        /// 
        /// To get continuos notification of fuel data changes request a subscription instead
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Parameter"></param>
        private void FuelDataRequest_handler(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            if (sender.GUID != null)
            {
                SendScriptCommand(sender.GUID, "FuelDataResponse", (Player.Character.isInVehicle() && Player.Character.CurrentVehicle.Metadata.Fuel != null) ? Player.Character.CurrentVehicle.Metadata.Fuel : null);
            }
        }
        /// <summary>
        /// Handler for the FuelDataSubscription Script Command
        /// 
        /// This commands subscribes the sender for continuos updates on CurrentFuelData changes
        /// 
        /// Bind to fuel data FuelDataSubscription_update command to obtain notifications from this subscription
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Parameter"></param>
        private void FuelDataSubscription_handler(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            if (sender.GUID != null)
            {
                // Check if the requesting script is already subscribed
                foreach (Guid guid in FuelDataSubscribers)
                    if (guid == sender.GUID)
                        // Return if the sender is already subscribed
                        return;
                // Subscribe the sender
                FuelDataSubscribers.Add(sender.GUID);
            }
        }

        #endregion Script Commands

        /// <summary>
        /// Updates model.CurrentFuelData and notifies subscribers of the latest changes
        /// </summary>
        /// <param name="newFuelData"></param>
        private void UpdateFuelData(FuelData newFuelData)
        {
            model.CurrentFuelData = newFuelData;
            
            // Notify subscribers
            foreach (Guid guid in model.FuelDataSubscribers)
                if (isScriptRunning(guid))
                    SendScriptCommand(guid, "FuelDataSubscription_update", newFuelData);

        }
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
                    Game.Console.Print(String.Format("{0}: {1}", methodName, message));
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