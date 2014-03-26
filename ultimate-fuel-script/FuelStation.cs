using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTA;

namespace ultimate_fuel_script
{
    public enum StationType
    {
        CarAndBike  = "",
        Boat = "BOAT",
        Helicopter = "HELI"
    }

    public class FuelStation
    {
        public FuelStation(string Name, float Radius, Vector3 Location, int WantedStars, StationType Type, bool DisplayBlip, float Price)
        {
            this.Name = Name;
            this.Radius = Radius;
            this.Location = Location;
            this.WantedStars = WantedStars;
            this.Type = Type;
            this.DisplayBlip = DisplayBlip;
            this.Price = Price;
        }

        #region Static Properties

        public static List<FuelStation> Items = new List<FuelStation>();

        #endregion Static Properties

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
        public StationType Type
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
                case StationType.CarAndBike:
                default:
                    StationBlip.Icon = (BlipIcon)79;
                    break;
                case StationType.Helicopter:
                    StationBlip.Icon = (BlipIcon)56;
                    break;
                case StationType.Boat:
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

    }
}
