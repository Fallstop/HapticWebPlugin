# Plugin Status

Each plugin can be in one of the following states:

- "Normal" - plugin is working properly.
- "Warning" - plugin is partially working:

Plugin status - warning

<!-- image -->

- "Error" - plugin is not working, e.g.:
    - Application is not installed.
    - Cannot connect to cloud service.
    - Login or authentication required.
    - Cannot connect to application plugin.

Plugin status - error

<!-- image -->

By default plugin is in "Normal" state.

Plugin developer should call `OnPluginStatusChanged` method to change plugin state.

## Plugin API

The following methods are available:

```
public void OnPluginStatusChanged ( PluginStatus status , String message );

public void OnPluginStatusChanged ( PluginStatus status , String message , String supportUrl , String supportUrlTitle );
```

- To set "normal" state:

```
this . OnPluginStatusChanged ( PluginStatus . Normal , null );
```

- To set "warning" state:

```
this . OnPluginStatusChanged ( PluginStatus . Warning , "Open the application." );
```

- To set "error" state:

```
this . OnPluginStatusChanged ( PluginStatus . Error , "Cannot connect to the application." , "https://support.loupedeck.com" , "Details" );
```

Example:

```
protected override void RunCommand ( String actionParameter )
{
if ( actionParameter . TryGetEnumValue < PluginStatus > ( out var pluginStatus ))
{
this . Plugin . OnPluginStatusChanged ( pluginStatus , $"Plugin status changed to {pluginStatus}." );
}
}
```