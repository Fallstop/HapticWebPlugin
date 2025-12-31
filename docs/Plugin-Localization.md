# Plugin Localization

Plugin can be localized to any language, even if this language is not supported by Logi Options+ or Loupedeck.

Normally plugin language follows the language of Logitech software (Logi Options+ or Loupedeck), but that can be changed by user in plugin properties or by plugin code (e.g. to match the language of the target application).

## Selecting plugin language

### Priority order

1. Forced plugin language (see [below](index.html#forced-plugin-language) ).
2. Plugin language set by plugin code, e.g. to match the connected application's language.
3. Current Logitech software language, if supported.
4. English (default) language.

### Using client application language

The plugin language can be defined by the language of the client application.

If it is not possible to get the language of the client application, the plugin language defaults to the same as the Logitech software language.

The plugin must set the current language as early as possible (for example, after establishing a connection with an application via the application API) using the `Plugin.Localization.SetCurrentLanguage()` method:

```
var applicationLanguageId = this . _applicationApi . GetLanguage ();

if ( ! this . Localization . SetCurrentLanguage ( applicationLanguageId ))
{
this . Localization . SetCurrentLanguage ( LocalizationEngine . DefaultLanguage );
}
```

### Forced plugin language

Forced plugin language can be used for testing plugin localization. Applying the setting requires Logi Plugin Service restart.

Add the following setting to `LoupedeckSettings.ini` file to force language for all plugins (except the default one):

```
Test/PluginLanguage = de-DE
```

### Language ID

Language ID is a string in the format `languagecode-countrycode` , where `languagecode` is a lowercase 2-letter language code derived from [ISO 639-1](https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes) and `countrycode` is derived from [ISO 3166-1 alpha-2](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2) and usually consists of two uppercase letters.

Examples are `en-US` , `en-GB` and `fi-FI` .

More information: [System.Globalization.CultureInfo.Name Property](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.name) .

## Localization files (XLIFF)

This section explains how to localize your plugin in any language. Note that the `en-US` language is a single source reference for the translations.

1. Generate XLIFF files with a deep link or [LogiPluginTool](../Getting-started/index.html) . Note that in both cases Logi Plugin Service should be running.
    - Using deep link: XLIFF files will be generated in the `localization.generated` subfolder of the plugin package. `loupedeck :// plugin /< plugin name >/ xliff` Example: `loupedeck :// plugin / spotify / xliff`
    - Using LogiPluginTool: XLIFF files will be generated to the specified directory. `LogiPluginTool xliff < plugin name > < directory path >` Example: `LogiPluginTool xliff Spotify ./`
2. Translate XLIFF files.
3. Put translated XLIFF files in `localization` subfolder of plugin package.
    - We recommend that file name consists of plugin name and language, e.g. `Spotify_de-DE.xliff` for German translation.
    - Check that `target-language` attribute of `<file>` tag contains the right language, e.g. `de-DE` for German translation.
4. Reload plugin: `loupedeck :// plugin /< plugin-name >/ reload` Example: `loupedeck :// plugin / Spotify / reload`
5. To verify translation, set required language, e.g. see [Forced plugin language](index.html#forced-plugin-language) .

### String IDs

String IDs are unique identifiers for localization that link text across different language files. In the Logi Actions SDK, English text strings automatically become their own string IDs and should not be changed once used in translations.

**Why this matters** : Once you have translation files, changing English text in your code will break the connection to existing translations. If you need to modify English text, you must create an English-to-English translation file instead.

**Example** : To change "Volume Up" to "Increase Volume", keep the original code unchanged (e.g. `this.DisplayName = "Volume Up";` ) and add a translation entry that maps `"Volume Up"` → `"Increase Volume"` in your English XLIFF file.

This approach preserves existing translations in other languages while updating the displayed English text.

## Advanced topics

### Mark action as non-localizable

Sometimes display names and descriptions of some commands and adjustments should not be localized (should not end up in generated XLIFF file).

In this case use the `SetLocalize(Boolean)` method:

```
public class ButtonSwitchesCommand : PluginDynamicCommand
{
public ButtonSwitchesCommand () : base ()
{
this . SetLocalize ( false );
}
}
```

Same applies to action parameters:

```
public class ButtonSwitchesCommand : PluginDynamicCommand
{
public ButtonSwitchesCommand () : base ()
{
this . AddParameter ( actionParameter , $"Switch {i}" , "Switches" ). SetLocalize ( false );
}
}
```

### Mark all action parameters as non-localizable

Sometimes action parameters are added at runtime, and their display names should not be localized (should not end up in generated XLIFF file). However it is still needed to localize the action itself.

In this case use the `SetLocalizeParameters(Boolean)` method:

```
public class ButtonSwitchesCommand : PluginDynamicCommand
{
public ButtonSwitchesCommand () : base ()
{
this . SetLocalizeParameters ( false );
}
}
```