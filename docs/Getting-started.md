# Getting Started

The Logi Actions SDK includes a command-line tool ( `LogiPluginTool` ) that allows developers to create plugin projects, package plugins, and verify them.

## Prerequisites

- Basic knowledge of .NET and C# development.
- A .NET IDE (e.g., Visual Studio Code, Visual Studio 2022 Community Edition or higher, or JetBrains Rider).
- A device for testing the plugin (e.g., Logitech MX Creative Console, Loupedeck CT, Loupedeck Live, Loupedeck Live S, or Razer Stream Controller).

## Installation and First Build

1. Ensure you have the latest Logitech Options+ or Loupedeck software installed:
    - Logitech Options+: [https://www.logitech.com/software/logi-options-plus.html](https://www.logitech.com/software/logi-options-plus.html)
    - Loupedeck Software: [https://loupedeck.com/downloads/](https://loupedeck.com/downloads/)
2. Install the latest .NET 8 SDK:
    - You can download it from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
3. Install the LogiPluginTool as a .NET tool. Open a terminal and run the following command: `dotnet tool install --global LogiPluginTool`
4. Generate a template plugin project using the LogiPluginTool: `logiplugintool generate Example` where "Example" is the name of the plugin. The command creates a folder named `ExamplePlugin` in the current directory.
5. Navigate to the generated folder and build the template plugin solution: `cd ExamplePlugin dotnet build`
6. Confirm that the build produces a `.link` file in the Logi Plugin Service's Plugins directory: Windows macOS `C :\ Users \ USERNAME \ AppDAta \ Local \ Logi \ LogiPluginService \ Plugins \ ExamplePlugin . link /Users/USERNAME/Library/Application Support/Logi/LogiPluginService/Plugins/ExamplePlugin.link` Note The `.link` file simply tells the Logi Plugin Service where to load your plugin from. If the installed plugin folder is present, the plugin from .link address is loaded first.
7. Launch Logitech Options+ or Loupedeck software and wait for the configuration UI to appear. In the Logitech Options+ configuration view, navigate to 'All Actions' and verify that 'ExamplePlugin' appears under the 'Installed Plugins' section. If the plugin is not shown on the list, go the Options+ settings and select 'Restart Logi Plugin Service'. In Loupedeck Software, Unhide the "Example" plugin on the "Hide and show plugins" tab of the Action panel. The plugin should be now shown in the UI.

## Hot Reloading

You can use the .NET Hot Reload feature to automatically rebuild the plugin project and reload the plugin in the host software whenever a source code file is saved.

To start hot reloading, first navigate to the plugin project's `src` directory, then run the watch command:

Windows macOS

```
cd ExamplePlugin \ src \
dotnet watch build
```

```
cd ExamplePlugin/src/ dotnet watch build
```

More information about .NET Hot Reload: [https://devblogs.microsoft.com/dotnet/introducing-net-hot-reload/](https://devblogs.microsoft.com/dotnet/introducing-net-hot-reload/)