# Icon Editor

Icon Editor is a part of the Options+ and Loupedeck user interfaces that lets users customize action icons. It provides navigation controls and tools, allowing you to select an icon and modify its appearance or associated text.

## Developer mode

For additional capability, developers can export Icon Templates via Icon Editor in developer mode:

1. Enable developer mode:
    - Stop Logi Plugin Service.
    - Open the `LoupedeckSettings.ini` configuration file located in the Logi Plugin Service directory:
        - Path (Windows): `C:\Users\<user_name>\AppData\Local\Logi\LogiPluginService`
        - Path (macOS): `~/Library/Application Support/Logi/LogiPluginService`
    - Add the following line: `Loupedeck/DeveloperMode = True`
    - Start Logi Plugin Service.
2. Open Icon Editor.
Open Icon Editor

<!-- image -->
4. Switch to developer mode by clicking on the "Edit Icon: ..." title in the Icon Editor interface.
Switch to developer mode by clicking on the "Edit Icon: ..." title

<!-- image -->
6. Perform needed updates and use "Export Icon" button to export the Icon Template.
Use "Export Icon" button to export the Icon Template

<!-- image -->