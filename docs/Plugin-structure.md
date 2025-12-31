# Plugin Structure

Plugin consists of core implementation classes and an organized package structure that defines functionality, appearance, and localization.

## Core classes

Plugin must implement both of these classes:

- `{PluginName}Plugin` class (inherited from the `Plugin` abstract class) contains the plugin-level logic.
- `{PluginName}Application` class (inherited from the `ClientApplication` abstract class) contains the logic related to the client application.

## Plugin package structure

Plugin can consist of several folders that provide different functionality. These folders should become a part of .lplug4 plugin installation package file.

### Required folders

#### metadata

Contains essential plugin configuration and assets:

- `LoupedeckPackage.yaml` - Plugin configuration file (see [Plugin Configuration File Structure](index.html#plugin-configuration-file-structure) ).
- `Icon256x256.png` - see [Plugin icon](../Plugin-Icon/index.html) .
- `DefaultIconTemplate.ict` - Optional default icon template for plugin-level branding (see [Icon Templates](../Icon-Templates/index.html) for detailed information).

### Optional folders

#### win

Binaries for Windows version of plugin. Add only if Windows is supported.

#### mac

Binaries for Mac version of plugin. Add only if Mac is supported.

#### actionicons

Plugins automatically retrieve icon image files from this folder using a predefined naming convention. The system searches for appropriately named image files that correspond to specific actions, eliminating the need for manual icon registration or configuration. This approach allows plugins to personalize the device experience with custom visual representations for each action.

Supported Formats:

- Raster images support: `.png` files with transparent backgrounds. Resolution should be optimized for device-specific button sizes.
- Vector images support: `.svg` files for scalable vector graphics.

See [Vector Images](../Vector-Images/index.html) for additional implementation details.

#### icontemplates

Contains action-specific icon templates ( `.ict` files) that define button appearance and layout:

- Files should be named using the action class full name (e.g. `Loupedeck.DemoPlugin.ToggleMuteCommand.ict` ).
- Templates can be created and exported using the [Icon Editor developer mode](../Icon-Editor/index.html#developer-mode) .

See [Icon Templates](../Icon-Templates/index.html) for detailed information.

#### actionsymbols

Plugin action symbols are small SVG icons appear to the left of action names in the action picker of the configuration UI that provide visual identification for plugin actions in the configuration interface. They enhance user experience by making actions easily recognizable and distinguishable from one another.

The system automatically discovers and loads symbols from the `actionsymbols` folder using a predefined naming convention, eliminating the need for manual registration.

#### profiles

Contains default application profiles ( `.lp5` files) that define the initial button layouts, actions, and configurations for your plugin when users first install it or create new profiles.

Profiles contain:

- Button mappings - which actions are assigned to which physical buttons.
- Action configurations - parameters and settings for each action.
- Layout definitions - visual arrangement and grouping of controls.
- Device-specific adaptations - optimized layouts for different devices.

Profiles are automatically applied in the following scenarios:

- First Installation: When a user installs your application plugin for the first time.
- New Profile Creation: When a user creates a new profile for your application.
- Profile Reset: When a user resets their profile to defaults.
- Device Addition: When a user adds a new device to their setup.

See [Default Application Profiles](../Default-Application-Profiles/index.html) for complete device-specific naming and implementation details.

#### localization

Contains translation files in XLIFF format for comprehensive multi-language support, enabling plugins to provide localized user interfaces across multiple languages.

Uses standard language ID format `languagecode-countrycode` (e.g. `en-US` , `en-GB` , `fi-FI` ).

Localization files can be generated from your plugin using either method:

- Deep link: `loupedeck://plugin/<plugin-name>/xliff` .
- LogiPluginTool: `LogiPluginTool xliff <plugin-name> <directory-path>` .

Translation Workflow:

1. Generate XLIFF files from your plugin.
2. Translate the generated files for target languages.
3. Place translated files in the `localization` folder with naming convention: `<PluginName>_<language-id>.xliff` .
4. Ensure `target-language` attribute matches the language ID.
5. Reload plugin: `loupedeck://plugin/<plugin-name>/reload` .

See [Plugin Localization](../Plugin-Localization/index.html) for complete implementation details.

#### events

Contains event definition files that enable plugins to define custom events that can trigger actions and workflows within the Logitech software. Events allow plugins to notify the system and other components when specific conditions or state changes occur.

See [Haptics Getting Started](../Haptics-Getting-Started/index.html) for additional implementation details.

## Plugin Configuration File Structure

The plugin configuration file `LoupedeckPackage.yaml` has the following format (the user-modifiable fields are in &lt;&gt; brackets)

```
type : plugin4
name : <Name of the plugin>
displayName : <Display name of the plugin>
version : <version string>
author : <author id>
copyright : <copyright>

supportedDevices : <Note if you support only one , remove another>
- LoupedeckCt
- LoupedeckLive

pluginFileName : <Plugin file name>
pluginFolderWin : <Folder for Windows binaries, add only if Windows is supported>
pluginFolderMac : <Folder for Mac binaries, add only if Mac is supported>
```

Mandatory fields:

- **type** : use "plugin4" for plugins.
- **name** : This is the unique ID for the plugin and cannot be changed after it's published in the marketplace. The field is limited to Latin small and capital letters, digits, underscore, and dash (regex: "[a-zA-Z0-9\_-]+"). Note! the name cannot contain "Plugin" at the end.
- **displayName** : Name that is shown in the Marketplace and in Options+ or Loupedeck software.
- **version** : is major.minor[.build] Every part must be a decimal number. Examples: "1.0", "1.0.0", If you're delivering an updated version of the plugin, please ensure the version number is increased accordingly.
- **author** : Name of the author that will be displayed in Marketplace.
- **supportPageUrl** : Could be an URL of a support page or a "mailto:" link of an email address. For example, GitHub issues can be used here for getting feedback on the plugin. Examples: https://support.mycompany.com/f-a-q-support or mailto:foo@bar.com
- **license** : Select a license under which you want to share the plugin. Please ensure that the selected license is compatible with Marketplace Developer License Agreement. One compatible option with the Marketplace is the MIT license: [The MIT License | Open Source Initiative](https://opensource.org/licenses/MIT) . **Note:** GPL licenses are not compatible with the Marketplace.
- **licenseUrl** : URL to license.

Optional fields:

- **copyright** : Author copyright.
- **backgroundColor** : Icon background color in ARGB format.
- **foregroundColor** : Icon foreground color in ARGB format.
- **textColor** : Icon text color in ARGB format.
- **supportedDevices** : use "- LoupedeckCt" for Loupedeck CT (and/or) "- LoupedeckLive" for Loupedeck Live (and/or) "- RazerStreamControllerX" for Razer Stream Controller X. If you support only one, remove another.
- **homePageUrl** : A link to a webpage, which has more information about the plugin.
- **icon256x256** : optional custom path and name to an icon file in the package. By default, the icon is searched from the 'metadata/Icon256x256.png' file.
- **minimumLoupedeckVersion** : Minimum Logi Plugin Service version that is required to run the plugin. The version is major.minor[.build] Every part must be a decimal number. Examples: "4.0", "4.0.0".

Here is an example `LoupedeckPackage.yaml` file for Spotify Premium plugin, which supports both Windows and Mac:

```
type : plugin4
name : SpotifyPremium
displayName : Spotify Premium
version : 1.0
author : Logitech
copyright : Logitech

backgroundColor : 4278869247
foregroundColor : 4294967295
textColor : 4294967295

supportedDevices :
- LoupedeckCt
- LoupedeckLive

pluginFileName : SpotifyPremiumPlugin.dll
pluginFolderWin : bin/win/
pluginFolderMac : bin/mac/

license : MIT
licenseUrl : https://opensource.org/licenses/MIT
homePageUrl : https://logitech.com
supportPageUrl : https://support.logitech.com/f-a-q-support
```