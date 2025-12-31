# Install And Uninstall

In addition plugin can override `Plugin.Install()` and `Plugin.Uninstall()` methods to customize installation and uninstallation process:

- `Install()` is called immediately after plugin is installed (copied to the Logi Plugin Service directory).
- `Uninstall()` is called just before plugin is uninstalled (deleted from the Logi Plugin Service directory).

What `Install()` method can do:

- Copy photo editing application plugin file(s) to specific directory.
- Copy scripts, presets, etc. to use by photo editing application plugin file(s) to specific directory.
- Allow specific TCP/IP ports in firewall.

What `Uninstall()` method can do:

- Delete files installed by Install method.
- Delete caching files.
- Install additional files

If plugin needs to install additional files (photo editing application plugin, scripts, presets, etc.) it can do that in `Install()` method:

- Additional files are added to plugin as "Embedded Resource".
- In `Install()` method plugin can call the following helper methods to extract these file to required directory:
    - `Assembly.ExtractFile(String resourceFileName, String pathName)` - extracts embedded resource file to specified location on local drive.
    - `Assembly.FindFile(String resourceFileName)` - return full path to embedded resource by file name.
    - `Assembly.GetFilesInFolder(String resourceFolderName)` - returns array of full paths to embedded resources located in given folder.
    - `Assembly.ReadTextFile(String resourceName)` - reads text from embedded resource text file.

Plugin has access to its Assembly instance via `this.Assembly` field:

```
var pluginFileName = Path.Combine(gimpDirectory, "plug-ins", "logi_plugin.py"); this.Assembly.ExtractFile("Loupedeck.Payload.plugin.logi_plugin.py", pluginFileName);
```

## Install dependencies

Logi Actions plugin should have a "copy" installation performed by copying plugin DLL to a plugin folder under Logi Plugin Service Plugins directory.

If your plugin requires additional class libraries, there are 2 ways to install these additional dependencies:

- Use [ILMerge](https://github.com/dotnet/ILMerge) tool (recommended).
- Copy dependency DLLs together with plugin DLL to the plugin folder and restart the service.

## Using installer tool

Starting with version 5.0 Logi Plugin Service supplies specific package installer that does all needed work to install the plugin to LogiPluginService Plugin directory and run all needed installation methods. The same tool LoupedeckPluginPackageInstaller.exe can uninstall the previously installed plugin.

The input for Logi Plugin Service Installer is a ZIP archive with `.lplug4` extension. To build it you can use the [Logi Plugin Tool](../Getting-started/index.html) .

See details here: [Distributing the plugin](../Distributing-the-plugin/index.html)