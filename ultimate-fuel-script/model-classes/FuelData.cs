using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTA;

namespace ultimate_fuel_script
{
    /// <summary>
    /// Keeps track of the current vehicle fuel data
    /// </summary>
    public class FuelData
    {
        public FuelData()
        {
            this.Tank = 100f;
            this.Reserve = 10f;
            this.Drain = 10f;
            this.Fuel = 10f;
        }

        #region Properties

        /// <summary>
        /// Stores vehicle tank capacity
        /// </summary>
        public float Tank
        { get; set; }
        /// <summary>
        /// Stores vehicle reserve level point
        /// </summary>
        public float Reserve
        { get; set; }
        /// <summary>
        /// Stores vehicle drain constant
        /// </summary>
        public float Drain
        { get; set; }
        /// <summary>
        /// Stores vehicle current fuel value
        /// </summary>
        public float Fuel
        { get; set; }
        /// <summary>
        /// Method of drain to use
        /// </summary>
        public StationType Type
        { get; private set; }

        public bool isFull
        {
            get
            {
                return this.Fuel >= this.Tank;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Generates a new instance of FuelData class to a given vehicle's metadata with data based on a given settings file
        /// </summary>
        /// <param name="veh"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static FuelData Generate(GTA.Vehicle veh, SettingsFile settings)
        {
            // Initialize FuelData object
            FuelData fuelData = new FuelData();
            // Defaults to 100
            fuelData.Tank = settings.GetValueFloat("TANK", veh.GetHashCode().ToString(), settings.GetValueFloat("TANK", veh.Name, 100));
            // Defaults to 10% of Tank
            fuelData.Reserve = settings.GetValueFloat("RESERVE", veh.GetHashCode().ToString(), settings.GetValueFloat("RESERVE", veh.Name, fuelData.Tank * .1f));
            // Defaults to ?somehing?
            fuelData.Drain = settings.GetValueFloat("DRAIN", veh.GetHashCode().ToString(), settings.GetValueFloat("DRAIN", veh.Name, 100));
            // Randomize fuel level
            fuelData.Fuel = new Random().Next((int)fuelData.Reserve + 1, (int)fuelData.Tank);
            // Assign type
            fuelData.Type = FuelStation.GetStationTypeFromVehicle(veh);
            // Return the attached data
            return fuelData;
        }


        public void DrainFuel(bool enableCars, bool enableHelicopters, bool enableBoats, Vehicle veh)
        {
            float Drain = 0.0f;
            switch (this.Type)
            {
                case StationType.CAR:
                    if (enableCars)
                    {
                        Drain = this.Drain * veh.CurrentRPM / 100;
                        Drain = Drain * ((1000 - veh.EngineHealth) / 1000) + Drain;
                        this.Fuel -= (Drain >= this.Fuel) ? this.Fuel : Drain;
                    }
                    break;
                case StationType.BOAT:
                    if (enableBoats)
                    {
                        if (controller.Gamepad == null)
                            if (Game.isGameKeyPressed(GameKey.MoveForward) ^ Game.isGameKeyPressed(GameKey.MoveBackward))
                                Drain = (this.Drain * (.2f + ((veh.Speed * .2f) / 5.0f))) / 100;
                            else
                                Drain = (this.Drain * .208f) / 100;
                        else
                            if (controller.Gamepad.GetState().Gamepad.RightTrigger > 0f ^ controller.Gamepad.GetState().Gamepad.LeftTrigger > 0f)
                                Drain = (this.Drain * (.2f + ((veh.Speed * .2f) / 5.0f))) / 100;
                            else
                                Drain = (this.Drain * .208f) / 100;

                        Drain = Drain * ((1000 - veh.EngineHealth) / 1000) + Drain;
                        this.Fuel -= (Drain >= this.Fuel) ? this.Fuel : Drain;
                    }
                    break;
                case StationType.HELI:
                    if (enableHelicopters)
                    {
                        if (controller.Gamepad == null)
                            if (Game.isGameKeyPressed(GameKey.MoveForward))
                                Drain = (this.Drain * (.2f + ((veh.Speed * .2f) / 5.0f))) / 100.0f;
                            else
                                Drain = (this.Drain * .208f) / 100.0f;
                        else if (controller.Gamepad.GetState().Gamepad.RightTrigger > 0.0f)
                            Drain = this.Drain * (((controller.Gamepad.GetState().Gamepad.RightTrigger * 100.0f) / 255.0f) / 10000.0f);
                        else
                            Drain = (this.Drain * .208f) / 100.0f;

                        Drain = Drain * ((1000 - veh.EngineHealth) / 1000.0f) + Drain;
                        this.Fuel -= (Drain >= this.Fuel) ? this.Fuel : Drain;
                    }
                    break;
            }
        }
        /// <summary>
        /// Adds a specified amount of fuel to the vehicle's fuel property until full
        /// </summary>
        /// <param name="amount"></param>
        public float AddFuel(float amount)
        {
            this.Fuel += amount;
            if (this.Fuel > this.Tank)
                this.Fuel = this.Tank;
            return amount;
        }
        #endregion

    }
}
