# Logging

Logging provides essential debugging and monitoring capabilities for both the Logi Plugin Service itself and individual plugins. The logging system helps developers troubleshoot issues, monitor plugin behavior, and track system events during development and production use.

## Log files location

Log files are located in the "Logs" subdirectory of the Logi Plugin Service data directory:

- Windows: `C:\Users\<user_name>\AppData\Local\Logi\LogiPluginService\Logs`
- macOS: `~/Users/<USERNAME>/Library/Application Support/Logi/LogiPluginService/Logs`

## Plugin logging

Logi Plugin Service provides logging possibilities also for plugins. The log messages from a plugin are written both to the Logi Plugin Service log file (when enabled) and to a plugin-specific log file. The plugin logging is always enabled, even if the Logi Plugin Service logging is disabled. The plugin log file is located in the `Logs\plugin_logs` subdirectory of the Logi Plugin Service data directory.

### PluginLog class

The `PluginLog` class provides helper methods to log messages easily everywhere in the plugin code.

You can find the source code here: [PluginLog.cs](https://github.com/Logitech/actions-sdk/blob/master/DemoPlugin/DemoPlugin/PluginLog.cs) .

The demo plugin contains an example of plugin logging by using `PluginLog` class: [DemoPlugin](https://github.com/Logitech/actions-sdk/blob/master/DemoPlugin/DemoPlugin/) .

The following log levels are supported by the plugin logs: `Verbose` , `Info` , `Warning` , `Error` .

For each log level the `PluginLog` class has two methods:

- For logging a message only: `public static void Info ( String text ) => PluginLog . _pluginLogFile ?. Info ( text );`
- For logging an exception and a message: `public static void Info ( Exception ex , String text ) => PluginLog . _pluginLogFile ?. Info ( ex , text );`

### Setting up logging for new plugins

For a new plugin, the easiest way to take the plugin logging into use is to generate the plugin project with the Logi Plugin Tool (see [Getting Started](../Getting-started/index.html) ). The Logi Plugin Tool version must be 5.6 or newer. The generated skeleton project contains the enabler code for plugin logging and an example of how to log messages from the plugin code.

### Setting up logging for existing plugins

For an existing plugin, you can take the plugin logging into use as follows:

1. Download the [PluginLog.cs](https://github.com/Logitech/actions-sdk/blob/master/DemoPlugin/DemoPlugin/PluginLog.cs) file and include it in your plugin project.
2. In the `PluginLog.cs` file, change the namespace to the same one that your plugin project uses: `namespace Loupedeck.DemoPlugin`
3. Initialize the `PluginLog` class in the constructor of your plugin class (replace the plugin class name `DemoPlugin` with your plugin class): `public DemoPlugin () => PluginLog . Init ( this . Log );`

After this, you can log messages in your plugin code:

```
PluginLog . Info ( "Counter was reset" );
```

## Logi Plugin Service logging

Logi Plugin Service logging is an advanced feature that plugin developers usually do not need. Plugin developers should generally use only plugin logging (described above), which provides comprehensive debugging capabilities for plugin-specific development. Logi Plugin Service logging is typically only required for deep system-level debugging or when working on complex issues.

### Enabling traces and logs

**Note:** Enabling logging might slow down the Logi Plugin Service considerably. Remember to turn off logging when you don't need it.

Enabling traces and logs for the Logi Plugin Service is done by creating empty files with specific names, without the file extension in the Logi Plugin Service data directory (same place where the LoupedeckSettings.ini file is located):

- `enablelogs` - enables writing traces to log file. Note: File name should not include a file extension. For example, enablelogs.txt will not work.