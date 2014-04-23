﻿using System;
using System.Diagnostics;
using System.IO;
using AdvancedHookManaged;
using GTA;
using GTA.Native;
using SlimDX.XInput;
using System.Windows.Forms;

namespace ultimate_fuel_script
{
    private enum ControllerButtons
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
        ControllerButtons RefuelButton;

        /// <summary>
        /// Keyboard action keys
        /// </summary>
        Keys RefuelKey;

        #endregion Properties

        public controller()
        {
            RefuelButton = ControllerButtons.BUTTON_A; // this is for test purposes only, this should selectable by the user
            RefuelKey = Keys.R;

            this.Interval = 250; // Run updates 4 times per second
            this.Tick += controller_Tick;
        }

        void controller_Tick(object sender, EventArgs e)
        {
            // Update current location
            model.CurrentFuelStation = FuelStation.IsAtStation(
                FuelStation.GetNearestStation(
                    Player.Character.Position,
                    FuelStation.GetStationTypeFromVehicle(Player.Character.CurrentVehicle)),
                Player.Character.Position);

            Actions tempAction = Actions.None;

            #region Enviroment based actions

            if (Player.Character.isInVehicle() && Player.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == Player.Character)
            {
                // Player is driving a vehicle
                tempAction = Actions.Driving;
                
                // Check if player is requesting a refuel process
                if (model.CurrentFuelStation != null && GTA.Native.Function.Call<bool>("IS_BUTTON_PRESSED", 0, (int)RefuelButton))
                    tempAction = Actions.Refueling;
            }

            #endregion Location based actions

            #region Input based actions

            if (model.CurrentFuelStation != null)
            {
                if (GTA.Native.Function.Call<bool>("IS_BUTTON_PRESSED", 0, (int)RefuelButton) || GTA.Native.Function.Call<bool>("IS_KEYBOARD_KEY_PRESSED", (int)RefuelKey))
                    model.UpdateCurrentAction(Actions.Refueling);
                else
                    model.UpdateCurrentAction(Actions.Driving);
            }

            #endregion Input based actions

            model.UpdateCurrentAction(tempAction);
        }
    }
}
