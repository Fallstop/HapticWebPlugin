# Plugin Capabilities

## Application and Universal Plugins

In terms of using applications, plugins are divided into two classes:

- The first class is the plugins for applications. These plugins are visible in the application section. They generally require an application to be in the foreground to execute commands.

Application plugins in Loupedeck Application

<!-- image -->

Application plugins in Logi Options+ Application

<!-- image -->

- The other type of plugins do not require an application to be in the foreground or any application to be running locally at all. For example, Twitch and Philips Hue plugins are using remote services directly.

Universal plugins in Loupedeck Application

<!-- image -->

Universal plugins in Logi Options+ Application

<!-- image -->

You can specify whether your plugin requires an associated application by changing the following flag in your Plugin class:

```
public override Boolean HasNoApplication => true ;
```

## Plugins with Shortcuts and API-Only Plugins

There are two distinct types of actions:

- Shortcuts, which essentially are key combinations that Logi Plugin Service sends on behalf of the user and
- API-based actions, those that are controlling target application/service using dedicated API (for example, OBS can be controlled via WebSocket using [obs-websocket plugin](https://github.com/Palakis/obs-websocket) )

To indicate if a plugin is having API-only actions, set the following flag in the Plugin class:

```
public override Boolean UsesApplicationApiOnly => true ;
```

Plugins with this flag set to true can be connected to any profile and are accessible from the Action Panel.

"Hide and show plugins" dialog in Loupedeck Application

<!-- image -->

## Plugin Configuration File

The plugin configuration file `LoupedeckPackage.yaml` can contain additional fields related to plugin capabilities.

### Plugin Capabilities Field

The `pluginCapabilities` field in `LoupedeckPackage.yaml` defines special installation and runtime requirements:

```
pluginCapabilities :
- RequiresAdminInstallation
- RequiresAdminUninstallation
- RequireApplicationCloseOnInstallWin
- RequireApplicationCloseOnInstallMac
```

**Available Capabilities:**

- `RequiresAdminInstallation` - Plugin requires installation with elevated rights
- `RequiresAdminUninstallation` - Plugin requires uninstallation with elevated rights
- `RequireApplicationCloseOnInstallWin` - Plugin requires application to be closed during installation and uninstallation (Windows). If it is defined, then `applicationPatterns` field is required
- `RequireApplicationCloseOnInstallMac` - Plugin requires application to be closed during installation and uninstallation (macOS). If it is defined, then `applicationPatterns` field is required

### Application Patterns Field

The `applicationPatterns` field defines regex expressions to identify supported applications (required when using application close capabilities):

```
applicationPatterns :
processNamePattern : ^lightroom$
bundleNamePattern : ^com.adobe.LightroomClassicCC7$
displayNamePattern : ^Adobe Lightroom Classic$
executablePathPattern : Adobe Lightroom Classic\\lightroom.exe$
```

**Pattern Types:**

- `processNamePattern` - Application process names (Windows)
- `bundleNamePattern` - Application bundle IDs (macOS)
- `displayNamePattern` - Application display name
- `executablePathPattern` - Application executable file path (Windows)

Uses .NET regex syntax: [Quick Reference](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference)