using System;
using System.Diagnostics;
using System.IO;
using AdvancedHookManaged;
using GTA;
using GTA.Native;
using System.Windows.Forms;
using System.Reflection;

namespace ultimate_indicator_script
{
    public class mainScript : Script
    {
        #region Properties
        internal Guid requesterGuid;
        internal bool requesterLock;
        /// <summary>
        /// Defines if the help message should be displayed
        /// </summary>
        internal bool showHelpMessage;

        internal static readonly string scriptName = "ultimate-indicator-script";
        #endregion Properties

        /// <summary>
        /// The script entry point
        /// </summary>
        public mainScript()
        {
            // Defauld script GUID
            GUID = new Guid("775df3cb-41c0-45f7-bd8f-d989853c838b");
            // Bind of external methods
            //
            //  Example of usage: 
            //
            //      SendScriptCommand(new Guid("775df3cb-41c0-45f7-bd8f-d989853c838b"), "HazardsOn", Player.Character.CurrentVehicle);
            //      
            //      Should turn the hazard lights for the vehicle the player is driving
            //
            BindScriptCommand("ResetAll", new ScriptCommandDelegate(ResetAll));
            BindScriptCommand("HazardsOn", new ScriptCommandDelegate(HazardsOn));
            BindScriptCommand("TurnLeft", new ScriptCommandDelegate(TurnLeft));
            BindScriptCommand("TurnRight", new ScriptCommandDelegate(TurnRight));

            showHelpMessage = Settings.GetValueBool("HELPMSG", "OPTIONS", true);

            if (Settings.GetValueString("MODE", "KEYS", "keyboard").ToLower().Trim() != "gamepad")
                BindKeys();

            // Set timer interval time
            this.Interval = 10;
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
            #region Handle help message display
            try
            {
                if (showHelpMessage && Player.Character.isInVehicle() && Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character)
                {
                    //Controller help
                    if (Settings.GetValueString("MODE").ToLower().Trim() != "keyboard" && Function.Call<bool>("IS_USING_CONTROLLER", new Parameter[0]))
                    {
                        mainScript.DisplayHelp("Use ~PAD_LSTICK_LEFTRIGHT_VEH~ and press ~PAD_X~ to activate the respective blinker. Press ~PAD_X~ again to turn off blinkers.", true);
                        showHelpMessage = false;
                    }
                    // Keyboard help
                    else if (Settings.GetValueString("MODE").ToLower().Trim() != "gamepad" && !Function.Call<bool>("IS_USING_CONTROLLER", new Parameter[0]))
                    {
                        mainScript.DisplayHelp(string.Format("Use {0} to activate right indicator, {1} to activate left indicator or {2} to activate hazard lights.", Settings.GetValueKey("RIGHT", Keys.E), Settings.GetValueKey("LEFT", Keys.Q), Settings.GetValueKey("HAZARDS", Keys.Z)), true);
                        showHelpMessage = false;
                    }
                }
            }
            catch (Exception E)
            {
                Log("Tick - display-help-message", E.Message);
            }
            #endregion Handle help message display

            #region Handle controller keys
            try
            {
                if (Settings.GetValueString("MODE", "KEYS", "keyboard").ToLower().Trim() != "keyboard" && Function.Call<bool>("IS_BUTTON_JUST_PRESSED", new Parameter[] { 0, 14 }) && Player.Character.isInVehicle() && Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character)
                {
                    GTA.Native.Pointer pointer = new GTA.Native.Pointer(typeof(int));
                    Function.Call("GET_POSITION_OF_ANALOGUE_STICKS", new Parameter[] { 0, pointer, new GTA.Native.Pointer(typeof(int)), new GTA.Native.Pointer(typeof(int)), new GTA.Native.Pointer(typeof(int)) });
                    int num = (int)pointer.Value;
                    if (num < 0)
                        this.TurnLeft(Player.Character.CurrentVehicle, this.GUID);
                    else if (num > 0)
                        this.TurnRight(Player.Character.CurrentVehicle, this.GUID);
                    else
                        this.ResetAll(Player.Character.CurrentVehicle, this.GUID);
                }
            }
            catch (Exception E)
            {
                Log("Tick - controller-key-handle", E.Message);
            }
            #endregion Handle controller keys
        }

        #region Methods


        /// <summary>
        /// Binds keyboard keys to indicator functions
        /// </summary>
        private void BindKeys()
        {
            try
            {
                // Bind right indicator key
                BindKey(Settings.GetValueKey("RIGHT", Keys.E), () =>
                {
                    // Check if player is in vehicle
                    // Check current indicator state
                    // Turn on case it's off
                    // Turn off case it's on
                    if (Player.Character.isInVehicle() && Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character)
                        if (GetMode(Player.Character.CurrentVehicle) == 2)
                            ResetAll(Player.Character.CurrentVehicle, this.GUID);
                        else
                            TurnRight(Player.Character.CurrentVehicle, this.GUID);
                });
                // Bind left indicator key
                BindKey(Settings.GetValueKey("LEFT", Keys.Q), () =>
                {
                    // Check if player is in vehicle
                    // Check current indicator state
                    // Turn on case it's off
                    // Turn off case it's on
                    if (Player.Character.isInVehicle() && Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character)
                        if (GetMode(Player.Character.CurrentVehicle) == 1)
                            ResetAll(Player.Character.CurrentVehicle, this.GUID);
                        else
                            TurnLeft(Player.Character.CurrentVehicle, this.GUID);
                });
                // Bind hazard lights key
                BindKey(Settings.GetValueKey("HAZARDS", Keys.Z), () =>
                {
                    // Check if player is in vehicle
                    // Check current indicator state
                    // Turn on case it's off
                    // Turn off case it's on
                    if (Player.Character.isInVehicle() && Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character)
                        if (GetMode(Player.Character.CurrentVehicle) == 3)
                            ResetAll(Player.Character.CurrentVehicle, this.GUID);
                        else
                            HazardsOn(Player.Character.CurrentVehicle, this.GUID);
                });
            }
            catch (Exception E)
            {
                Log("BindKeys", E.Message, false);
            }
        }
        /// <summary>
        /// Gets vehicle metadata for IndicatorLightsMode
        /// 
        /// Modes:
        ///     0 - All off
        ///     1 - Left indicator
        ///     2 - Right indicator
        ///     3 - Hazards on
        /// </summary>
        /// <param name="veh"></param>
        /// <returns></returns>
        private byte GetMode(Vehicle veh)
        {
            try
            {
                return veh.Metadata.IndicatorLightsMode;
            }
            catch (Exception)
            {
                // Most likely the vehicle does not have any metadata set for IndicatorLightsMode
                return SetMode(veh, 0);
            }
        }
        /// <summary>
        /// Sets the vehicle IndicatorLightsMode as vehicle metadata on the vehicle handle
        /// 
        /// Modes:
        ///     0 - All off
        ///     1 - Left indicator
        ///     2 - Right indicator
        ///     3 - Hazards on
        /// </summary>
        /// <param name="veh"></param>
        /// <param name="mode"></param>
        private byte SetMode(Vehicle veh, byte mode)
        {
            return veh.Metadata.IndicatorLightsMode = mode;
        }
        /// <summary>
        /// Resets all vehicle lights to their defaults
        /// </summary>
        /// <param name="veh"></param>
        private void ResetAll(Vehicle veh, Guid senderGuid, bool senderLock = false)
        {
            try
            {
                if ((requesterLock && requesterGuid == senderGuid) || !requesterLock)
                {
                    AVehicle aVehicle = TypeConverter.ConvertToAVehicle(veh);
                    MethodInfo method = aVehicle.GetType().GetMethod("IndicatorLight");
                    PropertyInfo property = aVehicle.GetType().GetProperty("IndicatorLightsMode");
                    (method.Invoke(aVehicle, new object[]
				    {
					    3
				    }) as AIndicatorLight).On = true;
                    (method.Invoke(aVehicle, new object[]
				    {
					    1
				    }) as AIndicatorLight).On = true;
                    (method.Invoke(aVehicle, new object[]
				    {
					    2
				    }) as AIndicatorLight).On = true;
                    (method.Invoke(aVehicle, new object[]
				    {
					    0
				    }) as AIndicatorLight).On = true;
                    property.SetValue(aVehicle, 0, null);

                    SetMode(veh, 0);

                    // Set the GUID lock
                    requesterLock = senderLock;
                    requesterGuid = senderGuid;
                }
            }
            catch (Exception ex)
            {
                mainScript.Log("ResetAll", ex.Message, false);
            }
        }
        /// <summary>
        /// Resets all vehicle lights to their defaults, for external script triggering
        /// </summary>
        /// <param name="veh"></param>
        private void ResetAll(GTA.Script sender, GTA.ObjectCollection Parameter)
		{
            try
            {
                Vehicle veh = Parameter.Convert<Vehicle>(0);
                bool senderLock = (Parameter.Count == 2) ? Parameter.Convert<bool>(1) : false;

                ResetAll(veh, sender.GUID, senderLock);
            }
            catch (Exception E)
            {
                Log("ResetAll - external-call", E.Message);
            }
		}
        /// <summary>
        /// Turns all indicator lights on in flashing mode
        /// </summary>
        /// <param name="veh"></param>
        private void HazardsOn(Vehicle veh, Guid senderGuid, bool senderLock = false)
		{
			try
            {
                if ((requesterLock && requesterGuid == senderGuid) || !requesterLock)
                {
                    AVehicle aVehicle = TypeConverter.ConvertToAVehicle(veh);
                    MethodInfo method = aVehicle.GetType().GetMethod("IndicatorLight");
                    PropertyInfo property = aVehicle.GetType().GetProperty("IndicatorLightsMode");
                    (method.Invoke(aVehicle, new object[]
				    {
					    3
				    }) as AIndicatorLight).On = true;
                    (method.Invoke(aVehicle, new object[]
				    {
					    1
				    }) as AIndicatorLight).On = true;
                    (method.Invoke(aVehicle, new object[]
				    {
					    2
				    }) as AIndicatorLight).On = true;
                    (method.Invoke(aVehicle, new object[]
				    {
					    0
				    }) as AIndicatorLight).On = true;
                    property.SetValue(aVehicle, 2, null);

                    SetMode(veh, 3);

                    // Set the GUID lock
                    requesterLock = senderLock;
                    requesterGuid = senderGuid;
                }
			}
			catch (Exception ex)
			{
				mainScript.Log("HazardsOn", ex.Message, false);
			}
		}
        /// <summary>
        /// Turns all indicator lights on in flashing mode, for external script triggering
        /// </summary>
        /// <param name="veh"></param>
        private void HazardsOn(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            try
            {
                Vehicle veh = Parameter.Convert<Vehicle>(0);
                bool senderLock = (Parameter.Count == 2) ? Parameter.Convert<bool>(1) : false;

                HazardsOn(veh, sender.GUID, senderLock);
            }
            catch (Exception E)
            {
                Log("HazardsOn - external-call", E.Message);
            }
        }
        /// <summary>
        /// Places in flashing mode and disables right hand side lights
        /// </summary>
        /// <param name="veh"></param>
        private void TurnLeft(Vehicle veh, Guid senderGuid, bool senderLock = false)
		{
			try
			{
                if ((requesterLock && requesterGuid == senderGuid) || !requesterLock)
                {
                    AVehicle aVehicle = TypeConverter.ConvertToAVehicle(veh);
                    MethodInfo method = aVehicle.GetType().GetMethod("IndicatorLight");
                    PropertyInfo property = aVehicle.GetType().GetProperty("IndicatorLightsMode");
                    (method.Invoke(aVehicle, new object[]
				    {
					    3
				    }) as AIndicatorLight).On = false;
                    (method.Invoke(aVehicle, new object[]
				    {
					    1
				    }) as AIndicatorLight).On = false;
                    (method.Invoke(aVehicle, new object[]
				    {
					    2
				    }) as AIndicatorLight).On = true;
                    (method.Invoke(aVehicle, new object[]
				    {
					    0
				    }) as AIndicatorLight).On = true;
                    property.SetValue(aVehicle, 2, null);

                    SetMode(veh, 1);

                    // Set the GUID lock
                    requesterLock = senderLock;
                    requesterGuid = senderGuid;
                }
			}
			catch (Exception ex)
			{
				mainScript.Log("TurnLeft", ex.Message, false);
			}
		}
        /// <summary>
        /// Places in flashing mode and disables right hand side lights, for external script triggering
        /// </summary>
        /// <param name="veh"></param>
        private void TurnLeft(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            try
            {
                Vehicle veh = Parameter.Convert<Vehicle>(0);
                bool senderLock = (Parameter.Count == 2) ? Parameter.Convert<bool>(1) : false;

                TurnLeft(veh, sender.GUID, senderLock);
            }
            catch (Exception E)
            {
                Log("TurnLeft - external-call", E.Message);
            }
        }
        /// <summary>
        /// Places in flashing mode and disables left hand side lights
        /// </summary>
        /// <param name="veh"></param>
        private void TurnRight(Vehicle veh, Guid senderGuid, bool senderLock = false)
		{
			try
			{
                if ((requesterLock && requesterGuid == senderGuid) || !requesterLock)
                {
                    AVehicle aVehicle = TypeConverter.ConvertToAVehicle(veh);
                    MethodInfo method = aVehicle.GetType().GetMethod("IndicatorLight");
                    PropertyInfo property = aVehicle.GetType().GetProperty("IndicatorLightsMode");
                    (method.Invoke(aVehicle, new object[]
				    {
					    3
				    }) as AIndicatorLight).On = true;
                    (method.Invoke(aVehicle, new object[]
				    {
					    1
				    }) as AIndicatorLight).On = true;
                    (method.Invoke(aVehicle, new object[]
				    {
					    2
				    }) as AIndicatorLight).On = false;
                    (method.Invoke(aVehicle, new object[]
				    {
					    0
				    }) as AIndicatorLight).On = false;
                    property.SetValue(aVehicle, 2, null);

                    SetMode(veh, 2);

                    // Set the GUID lock
                    requesterLock = senderLock;
                    requesterGuid = senderGuid;
                }
			}
			catch (Exception ex)
			{
				mainScript.Log("TurnRight", ex.Message, false);
			}
		}
        /// <summary>
        /// Places in flashing mode and disables left hand side lights, for external script triggering
        /// </summary>
        /// <param name="veh"></param>
        private void TurnRight(GTA.Script sender, GTA.ObjectCollection Parameter)
        {
            try
            {
                Vehicle veh = Parameter.Convert<Vehicle>(0);
                bool senderLock = (Parameter.Count == 2) ? Parameter.Convert<bool>(1) : false;

                TurnRight(veh, sender.GUID, senderLock);
            }
            catch (Exception E)
            {
                Log("TurnRight - external-call", E.Message);
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
