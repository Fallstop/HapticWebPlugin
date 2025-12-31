# Storing Plugin Data Locally

Logi Plugin Service provides a possibility for plugins to store data locally.

Use `Plugin.GetPluginDataDirectory()` method to get plugin data folder path.

Call `IoHelpers.EnsureDirectoryExists(String path)` method to ensure the given directory exists.

## Example

```
var pluginDataDirectory = this . GetPluginDataDirectory ();
if ( IoHelpers . EnsureDirectoryExists ( pluginDataDirectory ))
{
var filePath = Path . Combine ( pluginDataDirectory , "MyData.bin" );
using ( var streamWriter = new StreamWriter ( filePath ))
{
// Write data
}
}
```

## See Also

- [Managing Plugin Settings](../Managing-Plugin-Settings/index.html)