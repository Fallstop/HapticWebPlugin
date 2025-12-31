# External Service Login

Plugin account handling is managed through plugin preferences, where the plugin account is one of the available preference types. This system enables integration with external services that require user authentication.

Account preference should be created in the plugin constructor.

## Login and logout

When a user clicks login or logout button in configuration UI, plugin gets `PluginPreferenceAccount.LoginRequested` or `PluginPreferenceAccount.LogoutRequested` event correspondingly.

Good practice is to subscribe to these events in `Plugin.Load()` method and to unsubscribe from these events in `Plugin.Unload()` method.

When login or logout is done, plugin should call `PluginPreferenceAccount.ReportLogin()` or `PluginPreferenceAccount.ReportLogout()` method correspondingly.

## *Handling Access Denied errors*

If on any attempt to call an online service the plugin receives an *Access Denied* response, the plugin should call `PluginPreferenceAccount.ReportLogout()` method.

## Access and refresh tokens

**Access Token** : A short-lived credential that provides immediate authorization to access external service APIs. This token is used for authenticating API requests and typically expires after a limited time period for security purposes.

**Refresh Token** : A long-lived credential used to obtain new access tokens when they expire. This token allows the plugin to maintain authentication without requiring the user to log in again.

`PluginPreferenceAccount` class has `AccessToken` and `RefreshToken` properties that plugin can use to store access and refresh tokens.

- These properties are persistently stored between Logitech software sessions.
- These properties are set in the `PluginPreferenceAccount.ReportLogin()` method call and cleared in `PluginPreferenceAccount.ReportLogout()` method call.

## Example

```
public class MyPlugin : Plugin
{
private readonly PluginPreferenceAccount _myAccount ;

public MyPlugin ()
{
// Create an account preference
this . _myAccount = new PluginPreferenceAccount ( "my-account" )
{
DisplayName = "My account" ,
IsRequired = true ,
LoginUrlTitle = "Sign in" ,
LogoutUrlTitle = "Sign out"
};

// Add the preference to the list
this . PluginPreferences . Add ( this . _myAccount );
}

public override void Load ()
{
// Subscribe to login/logout requests
this . _myAccount . LoginRequested += this . OnMyAccountLoginRequested ;
this . _myAccount . LogoutRequested += this . OnMyAccountLogoutRequested ;
}

public override void Unload ()
{
// Unsubscribe from login/logout requests
this . _myAccount . LoginRequested -= this . OnMyAccountLoginRequested ;
this . _myAccount . LogoutRequested -= this . OnMyAccountLogoutRequested ;
}

private void OnMyAccountLoginRequested ( Object sender , EventArgs e )
{
// Login to external service to get access token and refresh token (if it exists)
// ...
// Set user name, access token and refresh token
this . _myAccount . ReportLogin ( "ExternalServiceUserName" , "ExternalServiceAccessToken" , "ExternalServiceRefreshToken" );
}

private void OnMyAccountLogoutRequested ( Object sender , EventArgs e )
{
// Logout from external service
// ...
// Clear user name, access token and refresh token
this . _myAccount . ReportLogout ();
}
}
```