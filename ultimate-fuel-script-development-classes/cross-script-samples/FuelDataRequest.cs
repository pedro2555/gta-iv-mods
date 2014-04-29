using GTA;
// Required to access FuelData class
using ultimate_fuel_script;

class FuelDataRequest : Script
{
    public FuelDataRequest()
    {
        // Bind the request response
        BindScriptCommand("FuelDataResponse", new ScriptCommandDelegate(FuelDataResponse_handler));
        // Request CurrentFuelData
        SendScriptCommand("775df3cb-41c0-45f7-bd8f-d989853c838b", "FuelDataRequest", null);
    }

    /// <summary>
    /// Handle responses to request
    /// </summary>
    private void FuelDataResponse_handler(GTA.Script sender, GTA.ObjectCollection Parameter)
    {
        // Assign fuel data object
        FuelData response = (FuelData)Parameter[0];
    }
}