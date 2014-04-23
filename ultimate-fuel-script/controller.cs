using System;
using System.Diagnostics;
using System.IO;
using AdvancedHookManaged;
using GTA;
using GTA.Native;
using SlimDX;

namespace ultimate_fuel_script
{
    /// <summary>
    /// Handles user input to action translations
    /// </summary>
    class controller : Script
    {
        #region Properties



        #endregion Properties

        public controller()
        {
            this.Interval = 250; // Run updates 4 times per second
            this.Tick += controller_Tick;
        }

        void controller_Tick(object sender, EventArgs e)
        {
            
        }
    }
}
