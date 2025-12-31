# Managing Plugin Settings

For each plugin, Logi Plugin Service stores a collection of setting names and values. Here are some characteristics of plugin settings:

- Both setting names and values are of `String` type.
- The setting names are case-insensitive.
- Plugin settings are persistent and are stored encrypted.
- Any setting can be marked to be backed up in the cloud for the logged-in Logi user.

## Usage

The following methods are available in the `Plugin` class.

### Read setting

```
protected Boolean TryGetPluginSetting ( String settingName , out String settingValue );
```

Returns a plugin setting.

Returns `true` if the setting exists and `false` otherwise.

If the setting does not exist, then `settingValue` is set to `null` .

### Write setting

```
protected void SetPluginSetting ( String settingName , String settingValue , Boolean backupOnline );
```

Saves a plugin setting.

Set `backupOnline` to `true` to backup this setting in the cloud and `false` to keep it only locally.

### Delete setting

```
protected void DeletePluginSetting ( String settingName );
```

Deletes a plugin setting.

### List all settings

```
protected String [] ListPluginSettings ();
```

Returns a list of plugin setting names.

## Example

```
private String GetUserId ()
{
const String SettingName = "UserId" ;

// first try to get existing user ID

if ( this . TryGetPluginSetting ( SettingName , out var existingUserId ))
{
return existingUserId ;
}

// if it does not exist, generate a new one and save it

var newUserId = Guid . NewGuid (). ToString ( "N" );

this . SetPluginSetting ( SettingName , newUserId , false );

return newUserId ;
}
```

Note: The way you access plugin settings methods depends on the context of your code:

- **Plugin-level code:** When your code is inside a class that inherits from `Plugin` class (such as your main plugin class), you can call the settings methods directly using `this` , e.g. `this.TryGetPluginSetting(settingName, out var settingValue)` .
- **Action-level code:** When your code is inside a class that inherits from classes such as `PluginDynamicCommand` , you need to access the settings methods through the `Plugin` property, e.g. `this.Plugin.TryGetPluginSetting(settingName, out var settingValue)` . This is because action classes have a reference to the plugin instance through their `Plugin` property, rather than inheriting from the `Plugin` class directly.

## See Also

- [Storing Plugin Data](../Storing-Plugin-Data/index.html)