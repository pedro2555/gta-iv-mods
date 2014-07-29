using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ultimate_fuel_script
{
    /// <summary>
    /// Enumerations of XBox 360 button IDs
    /// </summary>
    public enum Buttons
    {
        BACK = 13,
        START = 12,
        X = 14,
        Y = 15,
        A = 16,
        B = 17,
        DPAD_UP = 8,
        DPAD_DOWN = 9,
        DPAD_LEFT = 10,
        DPAD_RIGHT = 11,
        TRIGGER_LEFT = 5,
        TRIGGER_RIGHT = 7,
        BUMPER_LEFT = 4,
        BUMPER_RIGHT = 6,
        STICK_LEFT = 18,
        STICK_RIGHT = 19
    }

    /// <summary>
    /// Enumerations of possible 
    /// </summary>
    enum GxtStrings
    {
        INPUT_DROP_WEAPON,
        INPUT_FRONTEND_ACCEPT,
        INPUT_FRONTEND_CANCEL,
        INPUT_FRONTEND_UP,
        INPUT_FRONTEND_DOWN,
        INPUT_FRONTEND_RB,
        INPUT_FRONTEND_X,
        INPUT_VEH_ACCELERATE,
        INPUT_VEH_BRAKE,
        INPUT_VEH_CIN_CAM,
        INPUT_VEH_HANDBRAKE,
        INPUT_VEH_HEADLIGHT,
        INPUT_VEH_NEXT_WEAPON
    }

    /// <summary>
    /// Represents and input set, matching keyboard keys to gamepad buttons
    /// </summary>
    public class InputSet
    {
        #region Properties

        /// <summary>
        /// The input set GXT name
        /// </summary>
        public string InputSetName
        { get; private set; }

        /// <summary>
        /// Keyboard key
        /// </summary>
        public Keys KeyboardKey
        { get; private set; }

        /// <summary>
        /// Gamepad button index
        /// </summary>
        public Buttons GamepadButton
        { get; private set; }

        #endregion Properties

        /// <summary>
        /// Initiate Input set from a Stream of input sets
        /// </summary>
        /// <param name="inputSetName"></param>
        /// <param name="inputSetSource"></param>
        public InputSet(string inputSetName, Stream inputSetSource)
        {
            // Default set is INPUT_DROP_WEAPON -- Keys.R -- Buttons.X
            this.InputSetName = "INPUT_DROP_WEAPON";
            this.KeyboardKey = Keys.R;
            this.GamepadButton = Buttons.X;
            try
            {
                StreamReader reader = new StreamReader(inputSetSource);
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    // Line is a comment jump over it
                    if (line.StartsWith("#"))
                        continue;
                    // Line matches the set name
                    if (line.StartsWith(inputSetName))
                    {
                        string[] lineElements = line.Split(',');
                        this.InputSetName = lineElements[0];
                        this.KeyboardKey = (Keys)Enum.Parse(typeof(Keys), lineElements[1]);
                        this.GamepadButton = (Buttons)Enum.Parse(typeof(Buttons), lineElements[2]);
                        break;
                    }
                }
            }
            catch (Exception E)
            {
                model.Log("new InputSet()", E.Message);
            }
        }
    }
}
