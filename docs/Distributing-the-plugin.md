# Distributing the Plugin

Plugins can be distributed via the Logitech Marketplace and Loupedeck Marketplace to all other users.

## Submitting a plugin to the Marketplace:

- Please ensure you have tested the plugin properly with the supported hardware and software.
- Ensure that your plugin complies with the [Marketplace Approval Guidelines](../Marketplace-Approval-Guidelines/index.html) .
- Ensure that the plugin icon is in the metadata/ -subfolder under the plugin folder.
- Pack the plugin to the .lplug4 file. The instructions can be found below.
- Deliver the plugin using the submission form at [https://marketplace.logitech.com/contribute](https://marketplace.logitech.com/contribute) .

## Packaging plugin to a .lplug4 file

A .lplug4 file is essentially a zip file with a specific format and a plugin configuration file. The file format is registered with Logi Plugin Service and can be installed by double-clicking the file.

To create a plugin package, use the Logi Plugin Tool with the pack command:

```
logiplugintool pack ./bin/Release/ ./Example.lplug4
```

To validate a package use Logi Plugin Tool with the verify command:

```
logiplugintool verify ./Example.lplug4
```

**.lplug4 packaging information:**

- Please check that the metadata file matches the claimed operating system support.
- Logi Plugin Service includes a package installer that does all the needed work to install the plugin to the Service Plugin directory and run all needed installation methods.
- Recommended name for the .lplug4 package: pluginName\_version.lplug4 example: SpotifyPremium\_1\_0.lplug4.
- The .lplug4 package must include a plugin configuration file named `LoupedeckPackage.yaml` in the metadata folder (see [Plugin Configuration File Structure](../Plugin-structure/index.html#plugin-configuration-file-structure) ).