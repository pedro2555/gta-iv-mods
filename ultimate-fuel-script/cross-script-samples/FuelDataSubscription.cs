using GTA;
// Required to access FuelData class
using ultimate_fuel_script;

class FuelDataSubscription : Script
{
    public FuelDataSubscription()
    {
        // Bind to subscription update notifications
        BindScriptCommand("FuelDataSubscription_update", new ScriptCommandDelegate(FuelDataSubscription_handler));
        // Request a subscription
        SendScriptCommand("775df3cb-41c0-45f7-bd8f-d989853c838b", "FuelDataSubscription", null);
    }

    /// <summary>
    /// Handle notification updates
    /// </summary>
    private void FuelDataSubscription_handler(GTA.Script sender, GTA.ObjectCollection Parameter)
    {
        // Assign fuel data object
        FuelData update = (FuelData)Parameter[0];
    }
}