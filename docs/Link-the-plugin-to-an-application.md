# Link the Plugin to an Application

You can link your Logi Actions plugin to an application so that the plugin is activated when the application comes to the foreground.

You can find the `DemoApplication` class here: [DemoApplication.cs](https://github.com/Logitech/actions-sdk/blob/master/DemoPlugin/DemoPlugin/DemoApplication.cs)

## Steps

1. Open the Demo plugin solution in Visual Studio.
2. In the Solution Explorer, double-click the DemoApplication.cs file.
3. Modify the `GetProcessName` method that returns the process name of the supported application. Replace `DemoApplication` with the name of the application you want to link to the plugin: `protected override String GetProcessName () => "DemoApplication" ;`
4. Start debugging and wait until the Options+ or Loupedeck software is loaded.
5. Open the configuration UI.
6. Turn on the *Adapt to App* .
7. Connect a console to your computer. The console shows the default profile on the screen. This is the default profile when no supported application is in the foreground.
8. Start the application you linked with the plugin.
9. The device shows the Demo profile on the screen.
10. Change the active applications with `Alt+Tab` to see how the device switches between them.

## Notes

1. It is possible to define several process names for the supported applications. In this case, override the `GetProcessNames` method instead of `GetProcessName` : `protected override String [] GetProcessNames () => new [] { "Ableton Live 10 Lite" , "Ableton Live 10 Standard" };`
2. You can set a process name filter instead of fixed application names. In this case, override the `IsProcessNameSupported` method instead of `GetProcessName` : `protected override Boolean IsProcessNameSupported ( String processName ) => processName . ContainsNoCase ( "CaptureOne" );`