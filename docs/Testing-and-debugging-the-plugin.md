# Testing and Debugging the Plugin

You can test your plugin by using it with the Logi Plugin Service.

The build task in the template project creates a .link file to the plugin installation folder. During the service startup, if this link file is found, the plugin is automatically loaded.

The plugin project generated with the Logi Plugin Tool has all the necessary settings in place. When you start debugging the generated plugin project in Visual Studio, the Logi Plugin Service is launched.

To debug your plugin, start the Logi Plugin Service using the built-in Visual Studio debugger. It is pre-configured in the project file.

1. To start debugging the plugin, switch the plugin solution to the Debug configuration.
2. Select Debug &gt; Start Debugging, or click Start on the toolbar.

You can set breakpoints, navigate code, inspect data, and do any other usual debugging activity.

You can debug the plugin in the same way as you [debug any other C# project](https://docs.microsoft.com/en-us/visualstudio/get-started/csharp/tutorial-debugger) .