# Dynamic Folders

A dynamic folder (also known as "Control center") is a dynamic workspace that is fully controlled by a plugin.

- Like a normal workspace, a dynamic folder contains touch pages, encoder pages and wheel tools.
- Unlike a normal workspace, users cannot add any items to it, the whole content is defined by a plugin.

A dynamic folder is represented in the UI by a command that can be assigned to any touch or physical button. A dynamic folder can be opened by pressing this button on the device.

A dynamic folder can be closed:

- by pressing a special "Back" button,
- by pressing the device Home button,
- by executing any change workspace or change page command, or
- when the active application is changed.

## Implementation

### Add command folder to plugin

To add a command folder to a plugin, the developer needs to:

1. Add a class inherited from `PluginDynamicFolder` class to the plugin project: `public class TaskSwitcherDynamicFolder : PluginDynamicFolder`
2. Set folder parameters in the constructor: `public TaskSwitcherDynamicFolder () { this . DisplayName = "Alt+Tab" ; this . GroupName = "System" ; }`
3. Override the `GetNavigationArea` method to change the default `ButtonArea` navigation mode: `public override PluginDynamicFolderNavigation GetNavigationArea ( DeviceType _ ) { return PluginDynamicFolderNavigation . EncoderArea ; }`
4. Define commands, adjustments and wheel tools that this workspace will contain.
5. Optionally, define actions display names, images and action behavior.

### Loading and unloading

- To execute some code at folder loading, override `Load` method (is called during plugin load).
- To execute some code at folder unloading, override `Unload` method (is called during plugin unload).

Do not execute any code in the dynamic folder constructor, except setting folder parameters.

### Activation and deactivation

- To execute some code at folder activation, override `Activate` method (the method is called when the first instance of the folder is open on the connected devices).
- To execute some code at folder deactivation, override `Deactivate` method (the method is called when the last instance of the folder is closed on the connected devices).

As an example, in `Activate` method you may want to subscribe to application events, and in `Deactivate` method to unsubscribe from those. The plugin does not need to process application events if the dynamic folder is not visible on the device.

### Folder button display name and image

By default, the button that opens a dynamic folder shows the dynamic folder display name defined in the constructor:

```
public NumpadDynamicFolder ()
{
this . DisplayName = "Numeric Pad" ;
}
```

However, it is possible to change the display name runtime by overriding `GetButtonDisplayName` method:

```
public override String GetButtonDisplayName ( PluginImageSize imageSize ) => $"{this.ChannelCount} Channels" ;
```

It is also possible to use an image instead of text in this button by overriding `GetButtonImage` method, for example:

```
protected override BitmapImage GetButtonImage ( PluginImageSize imageSize )
{
var bitmapImage = PluginResources . ReadImage ( "Loupedeck.DemoPlugin.Images.ButtonImage.png" );
return bitmapImage ;
}
```

These methods should return `null` if the display name or the image is not available.

See [Accessing plugin resource files](../Change-a-button-image/index.html#accessing-plugin-resource-files) for more information about using `PluginResources` class.

### Navigation modes

The following modes are available:

- `None` - navigation is fully done by the plugin developer.
- `ButtonArea` - "Back" button is automatically inserted on every touch page in the left top corner. This is the default mode.
- `EncodeArea` - "Back" button is automatically inserted at the top of the left encoder page area. This is possible only if the dynamic folder does not define any encoder actions (neither rotation nor reset ones).

Navigation mode can be changed by overriding the `GetNavigationArea` method.

### Adding navigation buttons in the code

If the navigation mode is set to `None` , then navigation buttons can be inserted in the code.

The following action names should be used:

- `PluginDynamicFolder.NavigateUpActionName` - for "Back" button;
- `PluginDynamicFolder.NavigateLeftActionName` - for "Previous Touch Page" button;
- `PluginDynamicFolder.NavigateRightActionName` - for "Next Touch Page" button;

### Define commands, adjustments and wheel tools

Use the following methods:

- `GetButtonPressActionNames` - to define touch button commands;
- `GetEncoderRotateActionNames` - to define encoder adjustments;
- `GetEncoderPressActionNames` - to define encoder "reset" commands;
- `GetWheelToolNames` - to define wheel tools.

The plugin developer should use these methods to create action names:

- `CreateCommandName` - for commands;
- `CreateAdjustmentName` - for adjustments.

A dynamic folder will automatically create more pages if the actions do not fit on one page.

A dynamic folder will automatically add navigation actions depending on the selected navigation mode.

```
public override IEnumerable < String > GetButtonPressActionNames ( DeviceType _ )
{
return new []
{
PluginDynamicFolder . NavigateUpActionName ,
this . CreateCommandName ( "7" ),
this . CreateCommandName ( "8" ),
this . CreateCommandName ( "9" ),
this . CreateCommandName ( "." ),
this . CreateCommandName ( "4" ),
this . CreateCommandName ( "5" ),
this . CreateCommandName ( "6" ),
this . CreateCommandName ( "0" ),
this . CreateCommandName ( "1" ),
this . CreateCommandName ( "2" ),
this . CreateCommandName ( "3" )
};
}
```

A dynamic page can inform the Logi Plugin Service that the list of actions or wheel tools has changed by calling these methods:

- `ButtonActionNamesChanged` - for commands (including "reset" commands of encoders)
- `EncoderActionNamesChanged` - for adjustments

### Define action display names and images

A dynamic folder can define display names and images for commands and adjustments by overriding the following methods:

- GetCommandDisplayName
- GetCommandImage
- GetAdjustmentDisplayName
- GetAdjustmentImage

These methods should return `null` if the display name or the image is not available.

The Logi Plugin Service first tries to get the image and then, if no image is available, uses the display name as the button image.

```
public override String GetCommandDisplayName ( String actionParameter , PluginImageSize imageSize ) =>
this . TryGetNativeApplication ( actionParameter , out var app ) ? app . DisplayName : "Unknown" ;

public override BitmapImage GetCommandImage ( String actionParameter , PluginImageSize imageSize ) =>
this . TryGetNativeApplication ( actionParameter , out var app ) ? app . Icon ?. ToImage () : null ;
```

### Define adjustment values

To return the adjustment value, override the `GetAdjustmentValue` method:

```
public override String GetAdjustmentValue ( String actionParameter ) =>
this . TryGetVolume ( actionParameter , out var volume ) ? volume . ToString ( "D" ) : null ;
```

To inform the Logi Plugin Service that the adjustment value has changed, call the `AdjustmentValueChanged` method:

```
private void OnVolumeChanged ( Object sender , VolumeMixerItemEventArgs e ) =>
this . AdjustmentValueChanged ( e . Id );
```

### Execute commands and apply adjustments

- `RunCommand` - override this method to execute commands
- `ApplyAdjustment` - override this method to apply adjustments

```
public override void RunCommand ( String actionParameter )
{
if ( Int32 . TryParse ( actionParameter , out var processId ))
{
// Implement process activation logic with the given processId
}
}
```

## Closing the Dynamic Folder

The folder can also be closed programmatically with `this.Close();` .

This might be useful when the dynamic folder is used to switch between several options. But please be consistent with the use. The recommendation is to either close the folder after each command or don't close it at all and let the user navigate out of it.

```
public override void RunCommand ( String actionParameter )
{
# execute the selected command here

# close the folder after the command
this . Close ();
}
```

There's no programmatic way to open the folder but the Logi Plugin Service controls this.

## Low level events

As an alternative to reacting to actions, the plugin developer might choose to react to low-level commands:

- `ProcessButtonEvent` - override to process physical button presses;
- `ProcessEncoderEvent` - override to process encoder rotations;
- `ProcessTouchEvent` - override to process touch events.
    - TouchDown
    - TouchUp
    - LongPress
    - LongRelease
    - Tap
    - DoubleTap
    - Move
    - HorizontalSwipe
    - VerticalSwipe
    - TwoFingerTap

If the dynamic folder handles a low-level event, it should return `true` .

```
public override Boolean ProcessButtonEvent ( String actionParameter , DeviceButtonEvent buttonEvent )
{
if ( buttonEvent . IsPressed )
{
this . SendKeyboardShortcut ( actionParameter );
return true ;
}

return false ;
}
```