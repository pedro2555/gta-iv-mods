Imports GTA
' Required to access FuelData class
Imports ultimate_fuel_script

Class FuelDataSubscription
    Inherits Script
    Public Sub New()
        ' Bind to subscription update notifications
        BindScriptCommand("FuelDataSubscription_update", New ScriptCommandDelegate(AddressOf FuelDataSubscription_handler))
        ' Request a subscription
        SendScriptCommand("775df3cb-41c0-45f7-bd8f-d989853c838b", "FuelDataSubscription", Nothing)
    End Sub

    ''' <summary>
    ''' Handle notification updates
    ''' </summary>
    Private Sub FuelDataSubscription_handler(sender As GTA.Script, Parameter As GTA.ObjectCollection)
        ' Assign fuel data object
        Dim update As FuelData = DirectCast(Parameter(0), FuelData)
    End Sub
End Class