Ultimate Indicator Script 1.0.0

Developed by pedro2555 <prodrigues1990@gmail.com>

This mod requires files by other authors:
	Scripthook v1.7.1.7b installed
	AdvancedHook.dll in your game directory

This mod provides you with the ability to use the indicator lights on any vehicle that supports them, both keyboard and gamepad are supported

This script allows other scripts to use it's functions, a brief example of this functionality is as follows:

	From you script you should call the 'SendScriptCommand' function, and give it:
		1. The GUID for this script - 775df3cb-41c0-45f7-bd8f-d989853c838b
		2. The function to call, they are:
			a. ResetAll
				Resets all indicators to their default state
			b. HazardsOn
				Puts all indicator lights in a flashing state
			c. TurnLeft
				Puts the left hand side indicator in a flashing state, !important, you should ensure that ResetAll is called after this function somewhere in the execution flow of your script
			d. TurnRight
				Puts the right hand side indicator in a flashing state, !important, you should ensure that ResetAll is called after this function somewhere in the execution flow of your script
		3. And a vehicle handle

	A less verbose example:

		SendScriptCommand(new Guid("775df3cb-41c0-45f7-bd8f-d989853c838b"), "HazardsOn", Player.Character.CurrentVehicle);

	Which should turn the hazard lights on the player's current vehicle.

Check http://github.com/pedro2555/gta-iv-mods/releases for newer versions and other mods.

Bugs and issues http://github.com/pedro2555/gta-iv-mods/issues
IMPORTANT: make sure to select the yellow label ultimate-indicator-script, otherwise it may take longer to analyze your request.