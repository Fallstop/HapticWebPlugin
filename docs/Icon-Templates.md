# Icon Templates

Icon Templates define button appearance by specifying its image and text layout. These templates improve customization and maintain clarity when handling button designs. Icon Template files have the `.ict` extension and can be used across various precedence levels.

## Icon Templates levels

Icon Templates operate on several levels of precedence (from high to low), which guide their application in different contexts:

1. User Level:
    - Scope: Can be updated directly by users in the [Icon Editor](../Icon-Editor/index.html) . Reset icon to default in the Icon Editor to remove the updates.
    - Location: Stored in the `ActionIcons` folder within the user profile directory.
    - Purpose: Allows personalized updates for icon designs unique to user-specific workflows.
2. Plugin Action Level (see [ToggleMuteCommand.cs](https://github.com/Logitech/actions-sdk/tree/master/DemoPlugin/DemoPlugin/ToggleMuteCommand.cs) and [its Icon Template](https://github.com/Logitech/actions-sdk/tree/master/DemoPlugin/DemoPlugin/package/icontemplates/Loupedeck.DemoPlugin.ToggleMuteCommand.ict) as an example):
    - Scope: Template configurations for an individual action. Icon Template can be created and exported using the [Icon Editor developer mode](../Icon-Editor/index.html#developer-mode) .
    - Location: Stored in the `icontemplates` folder of plugin packages. Action class full name should be used as the Icon Template file name.
    - Purpose: Provides plugin-specific customization and greater control over button appearance.
3. Plugin Level:
    - Scope: Default template configurations for individual plugins.
    - Location: `DefaultIconTemplate.ict` stored in the `metadata` folder of plugin packages.
    - Purpose: Ensures consistent plugin branding where specific configurations aren't defined.
4. Global Level:
    - Scope: The global default settings.
    - Location: Built into Logi Plugin Service and not editable by plugin developers.
    - Purpose: Acts as a fallback configuration to standardize appearance globally across plugins.

## Sample Icon Template

Here's a fully annotated example of an Icon Template:

```
{
"backgroundColor" : 4278869247 , // Background color in ARGB format
"items" : [
{
"$type" : "Loupedeck.Service.ActionIconImageItem, LoupedeckShared" , // Is being used for proper deserialization and is optional
"image" : "" , // Encoded image string if applicable
"imageFileName" : null , // Reference to image file, if available
"imageColor" : 4294967295 , // Tint color for the image in ARGB format
"imageRotation" : "None" , // Image rotation
"isVisible" : true , // Indicates whether the item is visible
"itemType" : "Image" , // Specifies that this item is an image
"area" : {
"x" : 15 ,
"y" : 0 ,
"width" : 70 ,
"height" : 70 ,
"isFullScreen" : true
} // Defines placement and size of the image
},
{
"$type" : "Loupedeck.Service.ActionIconTextItem, LoupedeckShared" , // Is being used for proper deserialization and is optional
"text" : "Some text" , // Default text display
"textColor" : 4294967295 , // Text color in ARGB format
"fontSize" : 5 , // Font size
"fontName" : "Brown Logitech Pan Light" , // Font type
"isVisible" : true , // Indicates whether the item is visible
"itemType" : "Text" , // Specifies that this item is a text component
"area" : {
"x" : 0 ,
"y" : 70 ,
"width" : 100 ,
"height" : 30 ,
"isFullScreen" : false
} // Defines placement and dimensions for the text
}
]
}
```

Key Notes

1. Icon Templates contain two optional items:
    - `"ItemType": "Image"` for visual buttons.
    - `"ItemType": "Text"` for text overlays on buttons.
2. `image` property is optional and can include encoded image data.
3. Visibility and placement are controlled with:
    - `area` properties ( `x` , `y` , `width` , `height` ) for positioning and sizing.
    - `isVisible` flags to determine whether an item is displayed.
4. Other properties such as `fontSize` , `imageRotation` , and `imageColor` enable additional customization.