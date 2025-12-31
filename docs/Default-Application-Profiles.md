# Default Application Profiles

Default application profiles should be used only with application plugins.

There are two cases where default application profiles are used:

- When a user installs the application plugin with the default profile for the first time
- When a user wants to create a new profile for the application

When Logi Plugin Service creates the application profile:

- First the service tries to use the default profile from the Logi Plugin Service application plugin
- If a default application profile is not available, Logi Plugin Service creates an empty profile

If the application plugin is updated and has a new version of the default profile:

- It's not automatically updated to the existing application profile
- The updated default profile acts as a template for new application profiles
- When the user creates a new application profile after the update, a new default application profile is used

## Creating Default profiles

To create `DefaultProfileXX.lp5` files, create the regular profile in the UI and use the export feature to save it as a DefaultProfile file:

- Create a new application profile in Logitech software (Logi Options+ or Loupedeck).
- From the profile dropdown, select the application and click the three dots next to it, then click "Add profile". Add actions from the plugin to the profile and create the layout.
- From the profile dropdown in the Logitech software, select the three dots next to the selected profile and select the profile you want to export. Then select the three dots next to it to select "Export profile".

Note: It is not recommended to edit the zip profile folders manually, as personal information, such as your account name, may remain visible.

## Naming

In most cases, we recommend using the default profile that extends to all devices, which name is:

- DefaultProfile20.lp5

If customization is wanted per device, you can use the following names:

- `DefaultProfile20.lp5` - for Loupedeck CT
- `DefaultProfile30.lp5` - for Loupedeck Live
- `DefaultProfile40.lp5` - for Razer Stream Controller
- `DefaultProfile50.lp5` - for Loupedeck Live S
- `DefaultProfile60.lp5` - for Razer Stream Controller X
- `DefaultProfile70.lp5` - for Logitech MX Creative Keypad
- `DefaultProfile71.lp5` - for Logitech MX Creative Dialpad
- `DefaultProfile72.lp5` - for Logitech Actions Ring

To make different default profiles for Windows and Mac, use the `win` and `mac` postfixes after the profile name. Example:

- `DefaultProfile20win.lp5` - for Loupedeck CT on Windows
- `DefaultProfile20mac.lp5` - for Loupedeck CT on Mac

## Location

### Plugin package

Recommended location for default profiles is [plugin package](../Plugin-structure/index.html) .

Put default profile files in the `profiles` subdirectory of plugin package root directory.

### Embedded resources

Legacy way to store default profiles is to put them as embedded resources in native plugin binary. Visual Studio or another IDE can be used.

We recommend adding the default profiles under `DefaultProfiles` folder, but it's not required.

[More information about embedding a resource to a project.](https://learn.microsoft.com/en-us/visualstudio/ide/build-actions)