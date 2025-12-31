# Vector Images

Logi Plugin Service can use vector images instead of raster images when drawing action icons on device.

Currently only [SVG](https://en.wikipedia.org/wiki/SVG) format is supported as a vector format.

Icon templates ( `.ict` files) or other plugin color settings can overwrite the SVG image file colors in the plugin only in case the SVG image file is monochrome, otherwise the SVG image file colors will be preserved. At the same time for both, the monochrome and multicolor SVG image files, icon background and text colors will be changed according to the settings.

## How to use vector images in plugins

### GetCommandImage and GetAdjustmentImage

- Images returned by `PluginDynamicCommand.GetCommandImage()` and `PluginDynamicAdjustment.GetAdjustmentImage()` methods can either have raster (PNG) or vector (SVG) format.

Below is an example of a plugin command that reads an SVG file from plugin assembly embedded resources and returns it as command image.

```
namespace Loupedeck.TestPlugin
{
using System ;

internal class VectorGraphicsDynamicCommand : PluginDynamicCommand
{
public VectorGraphicsDynamicCommand ()
: base ( "Vector graphics" , "Command that has an SVG image" , "Test Group" )
{
}

protected override BitmapImage GetCommandImage ( String actionParameter , PluginImageSize imageSize )
=> BitmapImage . FromResource ( this . Plugin . Assembly , "Loupedeck.TestPlugin.VectorGraphicsDynamicCommand.svg" );
}
}
```

### "actionicons" folder in LPLUG4 package

- If .LPLUG4 package has an `actionicons` folder in its root, then Logi Plugin Service first searches this folder for plugin action images.
- In this case no changes are required in plugin code.
- Images can either have raster (PNG) or vector (SVG) format.
- Image file name should consist of action class full name as its name and corresponding file extension, e.g. in the above example it should be "Loupedeck.TestPlugin.VectorGraphicsDynamicCommand.svg" ("Loupedeck.TestPlugin" from namespace name and `VectorGraphicsDynamicCommand` from class name).
- This is the preferred method.