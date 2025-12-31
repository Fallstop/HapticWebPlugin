# Add a Simple Adjustment

To create a simple adjustment, add to the plugin project a class inherited from the `PluginDynamicAdjustment` class. To alter the command appearance and behavior, change the properties and overwrite the virtual methods of this class.

As an example, let's add a simple adjustment to the Demo plugin that increases or decreases the counter based on the number of ticks with which the encoder is rotated.

You can find the `CounterAdjustment` class here: [CounterAdjustment.cs](https://github.com/Logitech/actions-sdk/blob/master/DemoPlugin/DemoPlugin/CounterAdjustment.cs)

## Steps

1. Open the Demo plugin solution in Visual Studio.
2. In the Solution Explorer, right-click on the DemoPlugin project and select Add &gt; Class.
3. Enter CounterAdjustment.cs as the file name and click Add. The `CounterAdjustment` class opens for editing.
4. Inherit the `CounterAdjustment` class from the `PluginDynamicAdjustment` class: `public class CounterAdjustment : PluginDynamicAdjustment`
5. Add a private counter field and set it to `0` : `private Int32 _counter = 0 ;`
6. Create an empty, parameterless constructor and set the command display name, description, and group name in the parent constructor parameters. To indicate that the adjustment has a reset functionality, set the parameter `hasReset` to `true` : `public CounterAdjustment () : base ( displayName : "Counter" , description : "Counts rotation ticks" , groupName : "Adjustments" , hasReset : true ) { }`
7. Overwrite the `ApplyAdjustment` method that is called every time a user rotates the encoder to which this adjustment is assigned: `protected override void ApplyAdjustment ( String actionParameter , Int32 diff ) { this . _counter += diff ; // Increase or decrease the counter by the number of ticks. }`
8. Overwrite the `RunCommand` method that is called every time a user presses the encoder to which this command is assigned: `protected override void RunCommand ( String actionParameter ) { this . _counter = 0 ; // Reset the counter. }`
9. Plugin Service can draw the current adjustment value near the encoder. To enable that functionality, overwrite the `GetAdjustmentValue` method: `protected override String GetAdjustmentValue ( String actionParameter ) => this . _counter . ToString ();`
10. To inform Plugin Service that the adjustment value has changed, call the `AdjustmentValueChanged` method: `protected override void ApplyAdjustment ( String actionParameter , Int32 diff ) { this . _counter += diff ; // Increase or decrease the counter by the number of ticks. this . AdjustmentValueChanged (); // Notify the Plugin service that the adjustment value has changed. } protected override void RunCommand ( String actionParameter ) { this . _counter = 0 ; // Reset the counter. this . AdjustmentValueChanged (); // Notify the Plugin service that the adjustment value has changed. }`
11. Start debugging and wait until the Software is loaded.
12. Open the configuration UI.
13. Turn off the *Adapt to App* .
14. In the applications dropdown list, select Demo.
15. On the left pane, under Rotation Adjustments, expand the Demo node, then expand the `Adjustments` group and ensure that the Counter adjustment is there.
16. Drag and drop the Counter command to any encoder.
17. Connect a console to your computer. The console shows the Counter command on the encoder screen.
18. Rotate the encoder to change the counter value.
19. Press the encoder to reset the counter value to `0` .