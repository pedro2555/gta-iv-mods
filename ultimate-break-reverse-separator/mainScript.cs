////////////////////////////////////////////////////////
///// AutoReversOff by EuGene v.1.11 (2015-05-02) /////
//////////////////////////////////////////////////////
///// Updated by Juansero29 v.2.0 (2023-01-10) //////
////////////////////////////////////////////////////

using System;
using AdvancedHookManaged;
using System.IO;
using GTA;
using GTA.Native;

namespace ultimatebreakreverseseparator
{
    public partial class DisableAutoReverse : Script
    {

        public DisableAutoReverse()
        {
            base.KeyDown += Main_KeyDown;
            base.Tick += Main_Tick;
            
            Wait(30);
        }

        private bool CarIsMovingFowards;
        private float MaxForwardSpeed;
        private float CurrentSpeed;
        private bool DidEmergencyStop = true;

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (PlayerIsInVehicleAndRightTriggerOrMoveForwardIsPressed())
            {
                CarIsMovingFowards = true;
            }


        }

        private bool PlayerIsInVehicleAndRightTriggerOrMoveForwardIsPressed()
        {
            return Player.Character.isInVehicle() && PlayerIsMovingForwardOrRightTriggerIsPressed();
        }

        private bool MovingBackwardsOrLeftTriggerPressed()
        {
            return Game.isGameKeyPressed(GameKey.MoveBackward) || Function.Call<bool>("IS_BUTTON_PRESSED", new Parameter[] { 0, 5 });
        }

        private bool PlayerIsMovingForwardOrRightTriggerIsPressed()
        {
            return Game.isGameKeyPressed(GameKey.MoveForward) || Function.Call<bool>("IS_BUTTON_PRESSED", new Parameter[] { 0, 7 });
        }

        private bool RunOnce = true;
        private void Main_Tick(object sender, EventArgs e)
        {
            if (Player.Character.isInVehicle())
            {
                if (RunOnce)
                {


                    RunOnce = false;
                    if (!Player.Character.CurrentVehicle.Model.isCar || !Player.Character.CurrentVehicle.Model.isBike)
                    {
                        CarIsMovingFowards = false;
                        return;
                    }
                    Log(nameof(Main_Tick), "RUN ONCE HAS EXECUTED", true);
                }


                if (Function.Call<bool>("IS_BUTTON_PRESSED", new Parameter[] { 0, 5 }))
                {
                    Log(nameof(Main_KeyDown), "IS_BUTTON_PRESSED: LEFT TRIGGER IS PRESSED", true);
                }
                if (PlayerIsInVehicleAndRightTriggerOrMoveForwardIsPressed())
                {
                    CarIsMovingFowards = true;
                    Log(nameof(Main_KeyDown), "IS_BUTTON_PRESSED: CarIsMovingForwards = True", true);

                }

                if (CarIsMovingFowards)
                {
                    CurrentSpeed = Player.Character.CurrentVehicle.Speed;

                    if (PlayerIsMovingForwardOrRightTriggerIsPressed())
                    {
                        MaxForwardSpeed = Player.Character.CurrentVehicle.Speed;
                    }

                    if (MovingBackwardsOrLeftTriggerPressed())
                    {
                        if (IsPlayerStopped() || CurrentSpeed > MaxForwardSpeed)
                        {

                            Log(nameof(Main_Tick), "CAR IS STOPPED WHILE GOING BACKWARDS, DOING EMERGENCY STOP", true);
                            CarIsMovingFowards = false;
                            ForceCarStop();
                        }
                        else
                        {
                            MaxForwardSpeed = CurrentSpeed;
                        }
                    }
                }
                else if (PlayerIsGoingForwardsAndIsStopped())
                {
                    if (!DidEmergencyStop)
                    {
                        Log(nameof(Main_Tick), "CAR IS STOPPED WHILE GOING FORWARDS, DOING EMERGENCY STOP", true);
                        ForceCarStop();
                    }
                }
            }
            else if (!RunOnce)
            {
                Function.Call("SET_PLAYER_CONTROL", Player, true);
                CarIsMovingFowards = false;
                RunOnce = true;
            }
        }

        private bool PlayerIsGoingForwardsAndIsStopped()
        {
            return !MovingBackwardsOrLeftTriggerPressed() && IsPlayerStopped();
        }

 

        /// <summary>
        /// More details at https://gtamods.com/wiki/02A0
        /// </summary>
        /// <returns>
        /// This conditional opcode returns true when the character is "stopped." The command name is somewhat misleading. 
        /// Being stopped includes standing still while on foot or in a vehicle, jumping without pressing the movement buttons, 
        /// releasing the sprint button while running, falling without pressing the movement buttons, and exiting a vehicle.
        /// </returns>
        private bool IsPlayerStopped()
        {
            return Function.Call<bool>("IS_CHAR_STOPPED", Player.Character);
        }

        private void ForceCarStop()
        {
            DidEmergencyStop = !DidEmergencyStop;
            Function.Call("SET_CAMERA_CONTROLS_DISABLED_WITH_PLAYER_CONTROLS", false);
            Function.Call("SET_PLAYER_CONTROL", Player, DidEmergencyStop);
            Function.Call("SET_EVERYONE_IGNORE_PLAYER", Player, false);
        }

        #region Helper Methods
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
                    using (StreamWriter streamWriter = File.AppendText(Game.InstallFolder + "\\scripts\\" + nameof(DisableAutoReverse) + ".log"))
                    {
                        streamWriter.WriteLine($"[{DateTime.Now:hh:mm:ss.fff}] @ {methodName}: {message}");
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }
            }
            catch
            {

            }
            finally
            {
                if (printToConsole)
                {
                    Game.Console.Print($"{methodName}: {message}");
                }
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
        #endregion

    }
}