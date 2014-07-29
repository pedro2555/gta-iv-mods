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
    /// <summary>
    /// Handles user input to action translations
    /// </summary>
    class controller : Script
    {
        #region Properties

        /// <summary>
        /// Input set
        /// </summary>
        public static InputSet RefuelInputSet
        { get; set; }

        /// <summary>
        /// Instance of XBox 360 controller to enable trigger press based fuel drain
        /// </summary>
        public static SlimDX.XInput.Controller Gamepad
        { get; private set; }

        #endregion Properties

        public controller()
        {
            // Initialize refuel input set
            controller.RefuelInputSet = new InputSet(Settings.GetValueString("REFUELSET", "CONTROLS", "INPUT_DROP_WEAPON"), System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ultimate_fuel_script.resources.input-sets.csv"));

            // Initiate gamepad
            controller.Gamepad = new SlimDX.XInput.Controller(UserIndex.One);
            if (!controller.Gamepad.IsConnected) controller.Gamepad = null;

            this.Interval = 100;
            this.Tick += controller_Tick;
        }

        void controller_Tick(object sender, EventArgs e)
        {
            // Update current location
            model.CurrentFuelStation = (Player.Character.isInVehicle()) ?
                FuelStation.IsAtStation(
                FuelStation.GetNearestStation(
                    Player.Character.Position,
                    VehicleType.GetVehicleTypeFromVehicle(Player.Character.CurrentVehicle)),
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
                if (Player.Character.CurrentVehicle.Speed < 1.5f && !model.CurrentFuelData.isFull && model.CurrentRefuelData.CanRefuel(Player.Money, Player.Character.CurrentVehicle, model.CurrentFuelStation) && (GTA.Native.Function.Call<bool>("IS_BUTTON_PRESSED", 0, (int)RefuelInputSet.GamepadButton) || Game.isKeyPressed(RefuelInputSet.KeyboardKey)))
                    tempAction = Actions.Refueling;
            }

            #endregion Input based actions
            
            model.UpdateCurrentAction(tempAction);
        }
    }
}