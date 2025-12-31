# Add a Simple Command

To create a simple command, add to your plugin project a class inherited from the `PluginDynamicCommand` class. To alter the command appearance and behavior, change the properties and overwrite the virtual methods of this class.

As an example, let's add a simple command to the Demo plugin that mutes and unmutes the system sound. You can assign this command to a touch or a physical button on a console.

To toggle between mute and unmute, we send the `VK_VOLUME_MUTE` [virtual-key code](https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes) using one of the native methods provided by the Logi Actions Plugin API that works on both Windows and macOS.

You can find the `ToggleMuteCommand` class here: [ToggleMuteCommand.cs](https://github.com/Logitech/actions-sdk/blob/master/DemoPlugin/DemoPlugin/ToggleMuteCommand.cs)

## Steps

1. Open the Demo plugin solution in Visual Studio.
2. In the Solution Explorer, right-click on the DemoPlugin project and select Add &gt; Class.
3. Enter ToggleMuteCommand.cs as the file name and click Add. The `ToggleMuteCommand` class opens for editing.
4. Inherit the `ToggleMuteCommand` class from the `PluginDynamicCommand` class: `class ToggleMuteCommand : PluginDynamicCommand`
5. Create an empty, parameterless constructor and set the command display name, description, and group name in the parent constructor parameters: `public ToggleMuteCommand () : base ( displayName : "Toggle Mute" , description : "Toggles audio mute state" , groupName : "Audio" ) { }`
6. Overwrite the `RunCommand` method that is called every time a user presses the touch or the physical button to which this command is assigned: `protected override void RunCommand ( String actionParameter ) { this . Plugin . ClientApplication . SendKeyboardShortcut ( VirtualKeyCode . VolumeMute ); }`
7. Start debugging and wait until the software is loaded.
8. Open the configuration UI.
9. Turn off the *Adapt to App* .
10. In the applications dropdown list, select Demo.
11. On the left pane, under Press Actions, expand the Demo node, then expand the Audio group and ensure that the Toggle Mute command is there.
12. Drag and drop the Toggle Mute command to any touch button.
13. Connect a console to your computer.
14. Check that the console shows the Toggle Mute command on the touch screen.
15. Press this button to mute and unmute your computer's audio.

Note: Actions can be grouped in the configuration UI:

- To add sub-groups, use three hash symbols `###` as a separator in the `groupName` parameter. `public ToggleMuteCommand () : base ( displayName : "Toggle Mute" , description : "Toggles audio mute state" , groupName : "Level1###Level2###Level3" ) { }`
- This approach can be used for different types of actions.
- Maximum number of group levels is 3.
Action group levels in the configuration UI

<!-- image -->