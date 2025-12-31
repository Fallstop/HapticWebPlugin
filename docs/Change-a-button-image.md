# Change a Button Image

By default, when the Logi Plugin Service needs to draw a button image, it uses the display name of the command that is assigned to the button.

However, the plugin can change this behavior so that an image is shown instead of the command name. To inform Logi Plugin Service that a custom image should be used, the `GetCommandImage` method of the `PluginDynamicCommand` class must be overridden.

Moreover, if the plugin wants to change a button image at runtime when the command state changes, the plugin can call the `ActionImageChanged` method to inform Logi Plugin Service that the image should be redrawn.

In the example below, we will create a simple dynamic command that changes its state when the user presses the button. When the command state changes, the command requests redrawing the button image.

You can find the `ThumbUpDownCommand` class here: [ThumbUpDownCommand.cs](https://github.com/Logitech/actions-sdk/blob/master/DemoPlugin/DemoPlugin/ThumbUpDownCommand.cs)

## Steps

1. First, create a `ThumbUpDownCommand` dynamic command that toggles the internal boolean `_isThumbDown` variable on every button press. See [Add a simple command](../Add-a-simple-command/index.html) for more information about creating dynamic commands. `namespace Loupedeck.DemoPlugin { using System ; public class ThumbUpDownCommand : PluginDynamicCommand { private Boolean _isThumbDown = false ; public ThumbUpDownCommand () : base ( displayName : "Thumb up/down" , description : null , groupName : "Switches" ) { } protected override void RunCommand ( String actionParameter ) { this . _isThumbDown = ! this . _isThumbDown ; } } }`
2. Add two images for "Thumb up" and "Thumb down" states to the plugin project. Build action for these files must be set to "Embedded Resource". The images "ThumbUp.png" and "ThumbDown.png" can be fetched from: [DemoPlugin/images](https://github.com/Logitech/actions-sdk/tree/master/DemoPlugin/DemoPlugin/images) Note that the buttonimages must be in PNG format and have a size of 80x80 pixels.
3. Add two string class members that will hold the image resource paths. In the constructor, set these members to the full path of these files. Note that call to `PluginResources.FindFile()` method eliminates the need to know the exact path to these files (see [Accessing plugin resource files](index.html#accessing-plugin-resource-files) ). `private readonly String _imageResourcePathThumbUp ; private readonly String _imageResourcePathThumbDown ; public ThumbUpDownCommand () : base ( displayName : "Thumb up/down" , description : null , groupName : "Switches" ) { this . _imageResourcePathThumbUp = PluginResources . FindFile ( "ThumbUp.png" ); this . _imageResourcePathThumbDown = PluginResources . FindFile ( "ThumbDown.png" ); }`
4. Override the `GetCommandImage` method to return the right image based on the command state: `protected override BitmapImage GetCommandImage ( String actionParameter , PluginImageSize imageSize ) { var resourcePath = this . _isThumbDown ? this . _imageResourcePathThumbDown : this . _imageResourcePathThumbUp ; return PluginResources . ReadImage ( resourcePath ); }`
5. Call the `ActionImageChanged` method when the command state changes: `protected override void RunCommand ( String actionParameter ) { this . _isThumbDown = ! this . _isThumbDown ; this . ActionImageChanged (); }` Note that calling `this.ActionImageChanged(null)` will redraw all the buttons currently shown on the device.

## Adding background image and text to button

Example:

- The image file must be added to the plugin project as an embedded resource.

```
protected override BitmapImage GetCommandImage ( String actionParameter , PluginImageSize imageSize )
{
using ( var bitmapBuilder = new BitmapBuilder ( imageSize ))
{
bitmapBuilder . SetBackgroundImage ( PluginResources . ReadImage ( "MyPlugin.EmbeddedResources.MyImage.png" ));
bitmapBuilder . DrawText ( "My text" );

return bitmapBuilder . ToImage ();
}
}
```

Note: Don't use optional `fontSize` parameter of the `DrawText` method when drawing text. Logi Plugin Service will define the best font size per device.

## Accessing plugin resource files

The `PluginResources` class provides helper methods to get plugin resources easily everywhere in the plugin code.

You can find the source code here: [PluginResources.cs](https://github.com/Logitech/actions-sdk/blob/master/DemoPlugin/DemoPlugin/PluginResources.cs) .

For instance, the following methods can be used for finding and getting images:

```
public static String FindFile ( String fileName ) => PluginResources . _assembly . FindFileOrThrow ( fileName );

public static BitmapImage ReadImage ( String resourceName ) => PluginResources . _assembly . ReadImage ( PluginResources . FindFile ( resourceName ));
```

### Setting up for new plugins

For a new plugin, the easiest way to take the plugin resources into use is to generate the plugin project with the Logi Plugin Tool (see [Getting Started](../Getting-started/index.html) ). The generated skeleton project contains the enabler code and an example of how to get plugin resources from the plugin code.

### Setting up for existing plugins

For an existing plugin, you can take the getting images into use as follows:

1. Download the [PluginResources.cs](https://github.com/Logitech/actions-sdk/blob/master/DemoPlugin/DemoPlugin/PluginResources.cs) file and include it in your plugin project.
2. In the `PluginResources.cs` file, change the namespace to the same one that your plugin project uses: `namespace Loupedeck.DemoPlugin`
3. Initialize the `PluginResources` class in the constructor of your plugin class (replace the plugin class name `DemoPlugin` with your plugin class): `public DemoPlugin () => PluginResources . Init ( this . Assembly );`

After this, you can get image resources in your plugin code:

```
String resourceName = PluginResources . FindFile ( "MyImage.png" );
BitmapImage myImage = PluginResources . ReadImage ( resourceName );
```