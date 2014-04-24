using System;
using System.Diagnostics;
using System.IO;
using AdvancedHookManaged;
using GTA;
using GTA.Native;
using SlimDX.XInput;
using System.Windows.Forms;

namespace ultimate_fuel_script
{
    enum Buttons
    {
        BUTTON_BACK = 13,
        BUTTON_START = 12,
        BUTTON_X = 14,
        BUTTON_Y = 15,
        BUTTON_A = 16,
        BUTTON_B = 17,
        BUTTON_DPAD_UP = 8,
        BUTTON_DPAD_DOWN = 9,
        BUTTON_DPAD_LEFT = 10,
        BUTTON_DPAD_RIGHT = 11,
        BUTTON_TRIGGER_LEFT = 5,
        BUTTON_TRIGGER_RIGHT = 7,
        BUTTON_BUMPER_LEFT = 4,
        BUTTON_BUMPER_RIGHT = 6,
        BUTTON_STICK_LEFT = 18,
        BUTTON_STICK_RIGHT = 19
    }

    /// <summary>
    /// Handles user input to action translations
    /// </summary>
    class controller : Script
    {
        #region Properties

        /// <summary>
        /// XBox360 controller action buttons
        /// </summary>
        private Buttons RefuelButton;

        /// <summary>
        /// Keyboard action keys
        /// </summary>
        private Keys RefuelKey;

        public static SlimDX.XInput.Controller Gamepad
        { get; private set; }

        #endregion Properties

        public controller()
        {

            RefuelButton = Buttons.BUTTON_A; // this is for test purposes only, this should selectable by the user
            RefuelKey = Keys.R;

            // Initiate gamepad
            if (Settings.GetValueBool("USEGAMEPAD", "CONTROLS", false))
            {
                controller.Gamepad = new SlimDX.XInput.Controller(UserIndex.One);
                if (!controller.Gamepad.IsConnected) controller.Gamepad = null;
            }
            else
                controller.Gamepad = null;

            this.Interval = 100; // Run updates 4 times per second
            this.Tick += controller_Tick;
        }

        void controller_Tick(object sender, EventArgs e)
        {
            // Update current location
            model.CurrentFuelStation = (Player.Character.isInVehicle()) ?
                FuelStation.IsAtStation(
                FuelStation.GetNearestStation(
                    Player.Character.Position,
                    FuelStation.GetStationTypeFromVehicle(Player.Character.CurrentVehicle)),
                    Player.Character.Position) :
                    null;

            Actions tempAction = Actions.None;
            
            #region Enviroment based actions

            if (Player.Character.isInVehicle() && Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character)
            {
                // Player is driving a vehicle
                tempAction = Actions.Driving;
            }

            #endregion Location based actions

            #region Input based actions

            if (model.CurrentFuelStation != null && Player.Character.isInVehicle())
            {
                // Handle a refuel request
                if (Player.Character.CurrentVehicle.Speed < 1.5f && !model.CurrentFuelData.isFull && model.CurrentFuelStation.CanRefuel(Player.Money) && (GTA.Native.Function.Call<bool>("IS_BUTTON_PRESSED", 0, (int)RefuelButton) || Game.isKeyPressed(RefuelKey)))
                    tempAction = Actions.Refueling;
            }

            #endregion Input based actions
            
            model.UpdateCurrentAction(tempAction);
        }
    }
}
