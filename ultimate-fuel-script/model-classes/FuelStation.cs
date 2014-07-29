﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTA;

namespace ultimate_fuel_script
{
    public class FuelStation
    {
        public FuelStation(string Name, float Radius, Vector3 Location, int WantedStars, VehicleTypes Type, bool DisplayBlip, float Price)
        {
            this.Name = Name;
            this.Radius = Radius;
            this.Location = Location;
            this.WantedStars = WantedStars;
            this.Type = Type;
            this.DisplayBlip = DisplayBlip;
            this.Price = Price;
        }

        #region Properties

        /// <summary>
        /// Station display name
        /// </summary>
        public string Name
        { get; private set; }
        /// <summary>
        /// Station action radius
        /// </summary>
        public float Radius
        { get; private set; }
        /// <summary>
        /// Station location
        /// </summary>
        public Vector3 Location
        { get; private set; }
        /// <summary>
        /// Number of wanted stars to add to player after refueling
        /// </summary>
        public int WantedStars
        { get; private set; }
        /// <summary>
        /// Vehicle type for this station
        /// </summary>
        public VehicleTypes Type
        { get; private set; }
        /// <summary>
        /// Map blip handle
        /// </summary>
        public Blip MapBlip
        { get; private set; }
        /// <summary>
        /// Gets if the Map Blip should be displayed
        /// </summary>
        public bool DisplayBlip
        { get; private set; }
        /// <summary>
        /// Fuel unit price
        /// </summary>
        public float Price
        { get; private set; }

        #endregion Properties

        #region Static Properties
        /// <summary>
        /// Holds a list of all fuel stations
        /// </summary>
        public static List<FuelStation> Items = new List<FuelStation>();

        public static float CarRefuelTick
        { get; private set; }

        public static float BoatRefuelTick
        { get; private set; }

        public static float HeliRefuelTick
        { get; private set; }

        #endregion Static Properties

        #region Methods

        /// <summary>
        /// Places a blip on the map removing an existing one, returns the placed Blip
        /// </summary>
        /// <returns></returns>
        public Blip PlaceOnMap()
        {
            // Try to remove an existing blip
            try
            {
                this.MapBlip.Delete();
            }
            catch (Exception) { }
            // Add a blip...
            Blip StationBlip = GTA.Blip.AddBlip(this.Location);
            // Place the appropriate icon 79 for Cars, 56 for Helis and 48 for Boats
            switch (this.Type)
            {
                case VehicleTypes.CAR:
                default:
                    StationBlip.Icon = (BlipIcon)79;
                    break;
                case VehicleTypes.HELI:
                    StationBlip.Icon = (BlipIcon)56;
                    break;
                case VehicleTypes.BOAT:
                    StationBlip.Icon = (BlipIcon)48;
                    break;
            }
            // Set the display name
            StationBlip.Name = this.Name;
            // Check if the blip should be easily visible
            StationBlip.Display = DisplayBlip ? BlipDisplay.MapOnly : BlipDisplay.Hidden;
            // Other settings
            StationBlip.Friendly = true;
            StationBlip.RouteActive = false;
            StationBlip.ShowOnlyWhenNear = true;

            return StationBlip;
        }

        #endregion Methods

        #region Static Methods

        /// <summary>
        /// Loads all types of stations
        /// </summary>
        public static void LoadStations(SettingsFile settingsFile)
        {
            try
            {
                // Clear any existing records
                FuelStation.Items.Clear();
                // Load all types
                foreach (VehicleTypes type in (VehicleTypes[])Enum.GetValues(typeof(VehicleTypes)))
                    LoadStations(type, settingsFile);
            }
            catch (Exception E)
            {
                model.Log("LoadStations", E.Message);
            }
        }
        /// <summary>
        /// Loads a specific type of station
        /// </summary>
        /// <param name="type"></param>
        public static void LoadStations(VehicleTypes type, SettingsFile settingsFile)
        {
            try
            {
                // Refuel tick
                switch (type)
                {
                    case VehicleTypes.CAR:
                        FuelStation.CarRefuelTick = settingsFile.GetValueFloat("CARREFUELTICK", .25f);
                        break;
                    case VehicleTypes.BOAT:
                        FuelStation.BoatRefuelTick = settingsFile.GetValueFloat("BOATREFUELTICK", .5f);
                        break;
                    case VehicleTypes.HELI:
                        FuelStation.HeliRefuelTick = settingsFile.GetValueFloat("HELIREFUELTICK", 1.0f);
                        break;
                }
                for (byte i = 1; i <= Byte.MaxValue; i++)
                {
                    // Get the station's location.
                    Vector3 stationLocation = settingsFile.GetValueVector3("LOCATION", type.ToString() + "STATION" + i,
                        new Vector3(-123456789.0987654321f, -123456789.0987654321f, -123456789.0987654321f));
                    if (stationLocation.X != -123456789.0987654321f && stationLocation.Y != -123456789.0987654321f && stationLocation.Z != -123456789.0987654321f)
                    {
                        FuelStation f = new FuelStation(
                            (settingsFile.GetValueString("NAME", type.ToString() + "STATION" + i, "Fuel Station").ToUpper().Trim().Length > 30) ?
                                settingsFile.GetValueString("NAME", type.ToString() + "STATION" + i, "Fuel Station").Trim().Substring(0, 29) :
                                settingsFile.GetValueString("NAME", type.ToString() + "STATION" + i, "Fuel Station").Trim(),
                            settingsFile.GetValueFloat("RADIUS", type.ToString() + "STATION" + i, 10.0f),
                            stationLocation,
                            settingsFile.GetValueInteger("STARS", type.ToString() + "STATION" + i, 0),
                            type,
                            settingsFile.GetValueBool("DISPLAY", type.ToString() + "STATION" + i, true),
                            settingsFile.GetValueFloat("PRICE", type.ToString() + "STATION" + i, 0.0f));
                        f.PlaceOnMap();
                        FuelStation.Items.Add(f);
                    }
                    else
                        break;
                }
            }
            catch (Exception E)
            {
                model.Log("LoadStations(Type)", E.Message);
            }
        }
        /// <summary>
        /// Gets the nearest station to a given origin
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static FuelStation GetNearestStation(Vector3 origin)
        {
            FuelStation res = null;
            foreach (FuelStation fs in FuelStation.Items)
                if (res == null || origin.DistanceTo(fs.Location) < origin.DistanceTo(res.Location))
                    res = fs;
            return res;
        }
        /// <summary>
        /// Gets the nearest station to a given origin of a given type
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static FuelStation GetNearestStation(Vector3 origin, VehicleTypes type)
        {
            FuelStation res = null;
            foreach (FuelStation fs in FuelStation.Items)
                if (fs.Type == type && (res == null || origin.DistanceTo(fs.Location) < origin.DistanceTo(res.Location)))
                    res = fs;
            return res;
        }
        /// <summary>
        /// Checks if a given location is located inside a given station, returns the given station if true, null if false
        /// </summary>
        /// <param name="station"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static FuelStation IsAtStation(FuelStation station, Vector3 location)
        {
            return (location.DistanceTo(station.Location) < station.Radius) ? station : null;
        }

        public static float GetRefuelTick(VehicleTypes type)
        {
            switch (type)
            {
                default:
                case VehicleTypes.CAR:
                    return FuelStation.CarRefuelTick;
                case VehicleTypes.BOAT:
                    return FuelStation.BoatRefuelTick;
                case VehicleTypes.HELI:
                    return FuelStation.HeliRefuelTick;
            }
        }

        #endregion Static Methods
    }
}