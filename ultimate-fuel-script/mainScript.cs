using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using AdvancedHookManaged;
using GTA;
using GTA.Native;
using System.Windows.Forms;
using SlimDX.XInput;

namespace ultimate_fuel_script
{
    public class mainScript : Script
    {
        #region Properties
        /// <summary>
        /// Control refueling state
        /// </summary>
        internal bool refueling = false;
        /// <summary>
        /// Gamepad instance for fuel calculation based on key press
        /// </summary>
        internal Controller GamePad;
        /// <summary>
        /// Holds the fuel station the player is currently in, null if not at a fuel station or if any fuel station diferent than the vehicle type
        /// </summary>
        internal FuelStation currentFuelStation = null;

        internal Gauge fuelGauge;
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
            this.Interval = 1000;
            // Assign timer tick event
            this.Tick += new EventHandler(mainScript_Tick);
            // Assign drawing frame event
            this.PerFrameDrawing += new GraphicsEventHandler(mainScript_PerFrameDrawing);

            #region Log script start
            mainScript.Log(" - - - - - - - - - - - - - - - STARTUP - - - - - - - - - - - - - - - ", String.Format("GTA IV {0} under {1}", Game.Version.ToString(), mainScript.getOSInfo()));
            mainScript.Log("Started", String.Format("{0} v{1}", mainScript.scriptName, FileVersionInfo.GetVersionInfo(Game.InstallFolder + "\\scripts\\" + mainScript.scriptName + ".net.dll").ProductVersion, true));
            #endregion Log script start

            try
            {
                // Define the GamePad if needed
                GamePad = (Settings.GetValueBool("GAMEPAD", "MISC", false)) ? new Controller(UserIndex.One) : null;
                // Initialize the fuel gauge
                fuelGauge = new Gauge(new PointF(Settings.GetValueFloat("X", "DASHBOARD", 0.0f), Settings.GetValueFloat("Y", "DASHBOARD", 0.0f)), Settings.GetValueFloat("W", "DASHBOARD", 0.11f));
                // Load fuel stations
                LoadStations();
            }
            catch (Exception E)
            {
                Log("mainScript - load", E.Message);
            }
        }

        /// <summary>
        /// Main tick event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mainScript_Tick(object sender, EventArgs e)
        {
            // no sense calculating nothing if player isn't driving a vehicle
            if (Player.Character.isInVehicle() && Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character)
            {
                // Force fuel data
                GetFuelLevel(Player.Character.CurrentVehicle);
                // Update fuel data
                DrainFuel(Player.Character.CurrentVehicle);

                #region proximity-detection

                // Check if player is in a vehicle, driving it and at slow speed
                if (Player.Character.CurrentVehicle.Speed < 1.5f)
                {
                    currentFuelStation = FuelStation.IsAtStation(
                        FuelStation.GetNearestStation(
                            Player.Character.Position,
                            FuelStation.GetStationTypeFromVehicle(Player.Character.CurrentVehicle)),
                        Player.Character.Position);
                    // Display an help message if necessary
                    if (currentFuelStation != null && displayHelpMessage)
                    {
                        // Display help message
                        if (currentFuelStation.DisplayBlip)
                        {
                            // Normal station message

                            DisplayHelp(String.Format("Welcome to ~y~{0}~w~. Hold ~INPUT_VEH_HANDBRAKE~ to refuel. ${1} per liter.",
                                currentFuelStation.Name,
                                currentFuelStation.Price));
                        }
                        else
                        {
                            // Hidden station message

                            DisplayHelp(String.Format("You found ~y~{0}~w~! Hold ~INPUT_VEH_HANDBRAKE~ to steal some fuel.",
                                currentFuelStation.Name));
                        }
                        // Message was already displayed
                        displayHelpMessage = false;
                    }
                    else
                        // Station changed, enable the help message again
                        displayHelpMessage = true;
                }
                #endregion proximity-detection
            }
        }

        /// <summary>
        /// Frame painting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainScript_PerFrameDrawing(object sender, GTA.GraphicsEventArgs e)
        {
            try
            {
                // If player is in a vehicle and driving it, paint the gauge
                if (Player.Character.isInVehicle() && Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character)
                    fuelGauge.Draw(e.Graphics, Player.Character.CurrentVehicle);
            }
            catch (Exception E)
            {
                Log("PerFrameDrawing", E.Message);
            }
        }

        #region Methods

        private void DrainFuel(Vehicle veh)
        {
            // Test if fuel tank is empty
            if (veh.Metadata.Fuel == 0)
            {
                veh.EngineRunning = false;
            }
            else
            {
                float Drain = 0.0f;
                // Draining enabled for cars and bikes?
                if ((veh.Model.isCar || veh.Model.isBike) && veh.EngineRunning && Settings.GetValueBool("CARS", "MISC", true))
                {
                    // Base calculation of drain value
                    Drain = veh.Metadata.Drain * veh.CurrentRPM / 100;
                    // Drain value increase based on engine health
                    Drain = Drain * ((1000 - veh.EngineHealth) / 1000) + Drain;
                    // Deduct from vehicle fuel avoiding negative values
                    veh.Metadata.Fuel -= (Drain >= veh.Metadata.Fuel) ? veh.Metadata.Fuel : Drain;
                }
                // Draining enabled for helicopters?
                else if (veh.Model.isHelicopter && Settings.GetValueBool("HELIS", "MISC", true))
                {
                    // Note: 254.921568627451f
                    // Note: 0.2 + ((speed * 0.2) / 5)
                    // Only take in account speed when accelerate xor reverse key is pressed.
                    // Check which input to use
                    if (GamePad == null)
                        if (Game.isGameKeyPressed(GameKey.MoveForward))
                            Drain = (veh.Metadata.Drain * (.2f + ((veh.Speed * .2f) / 5.0f))) / 100.0f;
                        else
                            Drain = (veh.Metadata.Drain * .208f) / 100.0f;
                    // Use the GamePad if available.
                    else if (GamePad.GetState().Gamepad.RightTrigger > 0.0f)
                        Drain = veh.Metadata.Drain * (((GamePad.GetState().Gamepad.RightTrigger * 100.0f) / 255.0f) / 10000.0f);
                    else
                        // Idle drain
                        Drain = (veh.Metadata.Drain * .208f) / 100.0f;

                    // Drain value increase based on engine health
                    Drain = Drain * ((1000 - veh.EngineHealth) / 1000.0f) + Drain;
                    veh.Metadata.Fuel -= Drain;
                    veh.Metadata.Fuel = (veh.Metadata.Fuel < .0f) ? .0f : veh.Metadata.Fuel;
                }
                // Draining enabled for boats?
                else if (veh.Model.isBoat && Settings.GetValueBool("BOATS", "MISC", true))
                {
                    // Note: 0.2 + ((speed * 0.2) / 5)
                    // Only take in account speed when accelerate xor reverse key is pressed.
                    // GamePad disabled or unavailable? Use keyboard!
                    if (GamePad == null)
                        if (Game.isGameKeyPressed(GameKey.MoveForward) ^ Game.isGameKeyPressed(GameKey.MoveBackward))
                            Drain = (veh.Metadata.Drain * (.2f + ((veh.Speed * .2f) / 5.0f))) / 100;
                        else
                            Drain = (veh.Metadata.Drain * .208f) / 100;
                    // Use the GamePad if available.
                    else
                        if (GamePad.GetState().Gamepad.RightTrigger > 0f ^ GamePad.GetState().Gamepad.LeftTrigger > 0f)
                            Drain = (veh.Metadata.Drain * (.2f + ((veh.Speed * .2f) / 5.0f))) / 100;
                        else
                            Drain = (veh.Metadata.Drain * .208f) / 100;

                    // Calculate the draining speed also taking engine damage to an account.
                    Drain = Drain * ((1000 - veh.EngineHealth) / 1000) + Drain;
                    veh.Metadata.Fuel -= Drain;
                    veh.Metadata.Fuel = (veh.Metadata.Fuel < .0f) ? .0f : veh.Metadata.Fuel;
                }
                // formula simplification tests
                Game.DisplayText((veh.Metadata.Drain * (.2f + ((veh.Speed * .2f) / 5.0f))) / 100 + " " + ((.2f * veh.Speed / 5.0f) * veh.Metadata.Drain) / 100, 1500);
            }
        }

        /// <summary>
        /// Returns the current fuel level or randomizes a new one
        /// </summary>
        /// <param name="veh"></param>
        /// <returns></returns>
        private float GetFuelLevel(Vehicle veh)
        {
            try
            {
                // Try to return an already saved value
                return veh.Metadata.Fuel;
            }
            catch (Exception)
            {
                // Fuel level is not set, check ini entries
                // Set Tank entry
                veh.Metadata.Tank = Settings.GetValueInteger("TANK", veh.GetHashCode().ToString(), // Get data by hash code
                        Settings.GetValueInteger("TANK", veh.Name, 100)); // Get data by name or default to 100
                // Set Drain entry
                veh.Metadata.Drain = Settings.GetValueInteger("DRAIN", veh.GetHashCode().ToString(), // Get data by hash code
                        Settings.GetValueInteger("DRAIN", veh.Name, 10)); // Get data by name or default to 10
                // Set Reserve entry
                veh.Metadata.Reserve = Settings.GetValueInteger("RESERVE", veh.GetHashCode().ToString(), // Get data by hash code
                        Settings.GetValueInteger("RESERVE", veh.Name, 10)); // Get data by name or default to 10
                // Generate a random value for the fuel level between Reserve + 1 and Tank
                return veh.Metadata.Fuel = (int)new Random().Next(veh.Metadata.Reserve + 1, veh.Metadata.Tank);
            }
        }
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
                                Settings.GetValueString("NAME", type.ToString() + "STATION" + i, "Fuel Station").Trim().Substring(0, 29) :
                                Settings.GetValueString("NAME", type.ToString() + "STATION" + i, "Fuel Station").Trim(),
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
