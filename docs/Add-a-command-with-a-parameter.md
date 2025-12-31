# Add a Command with a Parameter

Commands and adjustments can contain parameters. As an example, the "apply develop profile" command in the Lightroom plugin takes the preset file name as a parameter.

Parameters are especially useful when you cannot determine the number of similar commands or adjustments at the development stage. Consider Windows 10/11 Volume Mixer - you cannot predict how many channels it will have on different PCs. However, a plugin can implement a single "toggle mute" command (or "change volume" adjustment) that takes the channel name as a parameter - and serve them all.

The plugin needs to indicate that the action has a parameter, and provide a list of available parameters.

The list of parameters can change at any moment (for example if a user started Spotify that added a channel to Volume Mixer), and there is a way to notify the console about the change.

A command or adjustment can have only one string parameter. If the plugin needs to store more data associated with a parameter, it should treat the parameter as an ID and keep an internal dictionary that links this ID to any related data.

Plugin service treats a parameter as a random string. It is the plugin's responsibility to keep these parameters unique for every action.

To create a command with a parameter, add to the plugin project a class inherited from the `PluginDynamicCommand` class (same as for a simple command). However, commands with parameters use a different base constructor.

As an example, let's add a simple command to the Demo plugin that toggles 4 switches.

You can find the `ButtonSwitchesCommand` class here: [ButtonSwitchesCommand.cs](https://github.com/Logitech/actions-sdk/blob/master/DemoPlugin/DemoPlugin/ButtonSwitchesCommand.cs)

## Steps

1. Open the Demo plugin solution in Visual Studio.
2. In the Solution Explorer, right-click on the DemoPlugin project and select Add &gt; Class.
3. Enter ButtonSwitchesCommand.cs as the file name and click Add. The `ButtonSwitchesCommand` class opens for editing.
4. Inherit the `ButtonSwitchesCommand` class from the `PluginDynamicCommand` class: `class ButtonSwitchesCommand : PluginDynamicCommand`
5. Create an empty, parameterless constructor that calls the parameterless constructor of the base class. You need to define the display name, description, and group name separately for each parameter. `public ButtonSwitchesCommand () : base () { }`
6. Add 4 parameters in the constructor using the `AddParameter` method: `public ButtonSwitchesCommand () : base () { for ( var i = 0 ; i < 4 ; i ++ ) { // parameter is the switch index var actionParameter = i . ToString (); // add parameter this . AddParameter ( actionParameter , $"Switch {i}" , "Switches" ); } }`
7. Add `_switches` Boolean array that keeps the current state of switches: `private readonly Boolean [] _switches = new Boolean [ 4 ];`
8. Overwrite the `RunCommand` method that is called every time a user presses the touch or the physical button to which this command is assigned: `protected override void RunCommand ( String actionParameter ) { if ( Int32 . TryParse ( actionParameter , out var i )) { // turn the switch this . _switches [ i ] = ! this . _switches [ i ]; // inform service that command display name and/or image has changed this . ActionImageChanged ( actionParameter ); } }`
9. Overwrite the `GetCommandDisplayName` method that is called every time Plugin Service needs to show a command on the console or the configuration UI. Note that if your command does not change the display name during runtime, you don't need to override this method. Plugin Service uses display names that the plugin specifies with the `ActionImageChanged` method in the class constructor. `protected override String GetCommandDisplayName ( String actionParameter , PluginImageSize imageSize ) { if ( Int32 . TryParse ( actionParameter , out var i )) { return $"Switch {i}: {this._switches[i]}" ; } else { return null ; } }`
10. Start debugging and wait until the software is loaded.
11. Open the configuration UI.
12. Turn off the *Adapt to App* .
13. From the applications dropdown list, select Demo.
14. On the right pane, under Press Actions, expand the Demo node, then expand the Switches group and ensure that it contains 4 Switch commands.
15. Drag and drop more than one Switch command to any touch button.
16. Connect a console to your computer.
17. Check that the console shows the Switch commands on the touch screen.
18. Press the buttons and check how their text changes.