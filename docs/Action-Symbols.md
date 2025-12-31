# Action Symbols

Plugin action symbols are small icons that represent the actions. The symbols are located next to the action names in the action picker of the configuration UI:

Plugin action symbols

<!-- image -->

## How to add symbols

1. **Prepare your symbol files** : Create vector images in [SVG](https://en.wikipedia.org/wiki/SVG) format for your plugin actions.
2. **Create one symbol per action** : Ensure there is 1 symbol for each action that your plugin implements. Missing symbols will be replaced with a generic one.
3. **Place symbols in the correct directory** : Locate the symbols in the `actionsymbols` directory of your [plugin package](../Plugin-structure/index.html) .
4. **Add parameter-specific symbols (optional)** : If your action has a fixed (compile time) number of parameters, you can create 1 symbol per parameter for more specific representation.
5. **Restart the service** : After adding action symbols, restart Logi Plugin Service to apply the changes.

## Image requirements

### File format

- Symbols must be in SVG format.
- Symbols must have a transparent background.
- Symbols must have a single color (black) strokes and fills.

### File naming

- Symbol files must have `.svg` file extension.
- Symbol files must have full name of the action they represent:
    - `Loupedeck.DemoPlugin.ToggleMuteCommand.svg` if action is defined in `ToggleMuteCommand` class located in `Loupedeck.DemoPlugin` namespace.
- If action has a fixed (compile time) number of parameters:
    - Symbols can be specified for each parameter.
    - If symbol is not found for this parameter, then symbol for the action itself is used.
    - File name consists of full name of the action (as above), 3 underscores as a separator and parameter name:
        - `Loupedeck.DemoPlugin.ButtonSwitchesCommand___1.svg` for the parameter `1` of action implemented in `ButtonSwitchesCommand` class located in `Loupedeck.DemoPlugin` namespace.
        - If this file is not found, then `Loupedeck.DemoPlugin.ButtonSwitchesCommand.svg` file can be used.