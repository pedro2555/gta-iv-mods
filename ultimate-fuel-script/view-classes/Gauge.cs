using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTA;
using System.Drawing;

namespace ultimate_fuel_script
{
    public class Gauge
    {
        public Gauge(PointF Location, float Width)
        {
            this.Location = Location;
            this.Width = Width;

            this.Flashing = 0;

            Background = new RectangleF(
                        Location.X - 0.0025f,
                        Location.Y - 0.003f,
                        Width,
                        0.0125f);
            Foreground = new RectangleF(
                        Location.X,
                        Location.Y,
                        (1 * (Width - 0.007f)) / 1,
                        0.006f);
            Display = new RectangleF(
                        Location.X,
                        Location.Y,
                        0,
                        0.006f);
        }

        /// <summary>
        /// Controls the flashing sequence
        /// </summary>
        public int Flashing
        { get; set; }
        /// <summary>
        /// Gauge screen location
        /// </summary>
        public PointF Location
        { get; private set; }
        /// <summary>
        /// Gauge width
        /// </summary>
        public float Width
        { get; private set; }

        private RectangleF Background, Foreground, Display;

        /// <summary>
        /// Draws the gauge to a given GTA.Graphics object
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="veh"></param>
        public void Draw(GTA.Graphics graphics, FuelData fuelData)
        {
            try
            {
                // Define drawing scale
                graphics.Scaling = FontScaling.ScreenUnits;

                // Draw fuel level meter's black background.
                graphics.DrawRectangle(Background, GTA.ColorIndex.Black);

                // Draw fuel level meter's dark grey foreground.
                graphics.DrawRectangle(Foreground, (GTA.ColorIndex)1);

                // Draw the front rectange widening how much fuel vehicle has.
                // Green as normal, and red when running on reserved.
                Display.Width = (fuelData.Fuel * (Width - 0.008f)) / fuelData.Tank;
                graphics.DrawRectangle(Display,
                        (fuelData.isOnReserve)
                            ? ((Flashing < 5)
                                ? (GTA.ColorIndex)1
                                : (GTA.ColorIndex)35
                            )
                        : (GTA.ColorIndex)50); // at this point, if we have issues about performance the color can very well be select when any of the 3 conditions change in the first place

                // Controls the Flashinging when on reserved fuel.
                // Strange, but it won't flash if we used Flashing++;
                // It's not strange it is expected behaviour: Flashing++ uses the variable Flashing and updates it with 1. ++Flashing updates the variable with 1 and then uses it.
                Flashing = (Flashing == 20) ? 0 : ++Flashing;
            }
            catch (Exception E)
            {

            }
        }
    }
}