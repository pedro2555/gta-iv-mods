Imports GTA
' Required to access FuelData class
Imports ultimate_fuel_script

Class FuelDataRequest
    Inherits Script
    Public Sub New()
        ' Bind the request response
        BindScriptCommand("FuelDataResponse", New ScriptCommandDelegate(AddressOf FuelDataResponse_handler))
        ' Request CurrentFuelData
        SendScriptCommand("775df3cb-41c0-45f7-bd8f-d989853c838b", "FuelDataRequest", Nothing)
    End Sub

    ''' <summary>
    ''' Handle responses to request
    ''' </summary>
    Private Sub FuelDataResponse_handler(sender As GTA.Script, Parameter As GTA.ObjectCollection)
        ' Assign fuel data object
        Dim response As FuelData = DirectCast(Parameter(0), FuelData)
    End Sub
End Class