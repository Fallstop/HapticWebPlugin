# Action Editor Actions

Action Editor actions allow plugin developers to create custom controls with configurable user interfaces. These controls appear when users assign a plugin action to a device button or dial, enabling users to configure the action's behavior through a simple interface.

Use Action Editor actions when your plugin needs user configuration:

- **Text Input** : Actions that send custom text or commands.
- **File Selection** : Actions that work with specific files or folders.
- **Option Selection** : Actions with multiple behavior modes.
- **Device Configuration** : Actions that control external devices with settings.
- **Dynamic Content** : Actions that adapt based on available data sources.

### Basic Structure

Action Editor actions follow this pattern:

```
public class MyActionEditorCommand : ActionEditorCommand
{
public MyActionEditorCommand ()
{
// Set basic properties
this . Name = "UniqueActionName" ;
this . DisplayName = "User-friendly name" ;
this . GroupName = "Category" ;
this . Description = "Brief description of functionality" ;

// Add controls for user configuration
this . ActionEditor . AddControlEx (
new ActionEditorTextbox ( "TextControl" , "Enter text:" ));
this . ActionEditor . AddControlEx (
new ActionEditorCheckbox ( "CheckboxControl" , "Enable feature:" ));

// Subscribe to events
this . ActionEditor . ListboxItemsRequested += this . OnListboxItemsRequested ;
this . ActionEditor . ControlValueChanged += this . OnControlValueChanged ;
}

protected override Boolean RunCommand ( ActionEditorActionParameters actionParameters )
{
// Use the configured values when the action executes
if ( actionParameters . TryGetString ( "TextControl" , out var text ))
{
// Perform action with user's configured text

return true ;
}

return false ;
}
}
```

## Action Types

### ActionEditorCommand

Use `ActionEditorCommand` for button actions that execute when pressed. These are ideal for operations like sending text, opening files, or triggering system commands.

```
public class SendTextCommand : ActionEditorCommand
{
private const String TextControlName = "Text" ;

public SendTextCommand ()
{
this . Name = "SendText" ;
this . DisplayName = "Send Text" ;
this . GroupName = "Action Editor" ;
this . Description = "Place text into an active text field" ;

this . ActionEditor . AddControlEx ( new ActionEditorTextbox ( TextControlName , "Text:" ));
}

protected override Boolean RunCommand ( ActionEditorActionParameters actionParameters )
{
if ( actionParameters . TryGetString ( TextControlName , out var text ))
{
// Implement text sending functionality

return true ;
}

return false ;
}
}
```

### ActionEditorAdjustment

Use `ActionEditorAdjustment` for rotary control actions that respond to continuous input. These are perfect for adjustments like volume control, mouse movement, or scrolling.

```
public class MouseScrollAdjustment : ActionEditorAdjustment
{
private const String IsInvertedControlName = "IsInverted" ;

public MouseScrollAdjustment () : base ( false )
{
this . Name = "MouseScroll" ;
this . DisplayName = "Mouse Scroll" ;
this . GroupName = "Action Editor" ;
this . Description = "Control mouse scroll wheel" ;

this . ActionEditor . AddControlEx ( new ActionEditorCheckbox ( IsInvertedControlName , "Invert Direction" ));
}

protected override Boolean ApplyAdjustment ( ActionEditorActionParameters actionParameters , Int32 diff )
{
if ( actionParameters . TryGetBoolean ( IsInvertedControlName , out var isInverted ))
{
if ( isInverted )
{
diff = - diff ;
}

// Implement mouse wheel scrolling functionality

return true ;
}

return false ;
}
}
```

## Controls

There are different control types. Each action can have one or more controls. Controls can have configuration options like `SetRequired` , `SetDefaultValue` , `SetValues` , `SetFormatString` .

### ActionEditorTextbox

Text input control for text entry.

```
this . ActionEditor . AddControlEx (
new ActionEditorTextbox ( name : "TextControl" , labelText : "Enter text:" )
. SetRequired ()
. SetPlaceholder ( "Type here..." ));
```

### ActionEditorListbox

Dropdown selection control with dynamic item population.

```
this . ActionEditor . AddControlEx (
new ActionEditorListbox ( name : "ListControl" , labelText : "Select option:" ));
```

### ActionEditorCheckbox

Boolean toggle control for true/false options.

```
this . ActionEditor . AddControlEx (
new ActionEditorCheckbox ( name : "CheckboxControl" , labelText : "Enable feature:" )
. SetDefaultValue ( false ));
```

### ActionEditorFileSelector

File browser control for selecting files.

```
this . ActionEditor . AddControlEx (
new ActionEditorFileSelector ( name : "FileControl" , labelText : "Select file:" )
. SetInitialDirectory ( Environment . GetFolderPath ( Environment . SpecialFolder . Desktop )));
```

### ActionEditorDirectorySelector

Directory browser control for selecting folders.

```
this . ActionEditor . AddControlEx (
new ActionEditorDirectorySelector ( name : "DirectoryControl" , labelText : "Select directory:" ));
```

### ActionEditorKeyboardKey

Keyboard shortcut capture control.

```
this . ActionEditor . AddControlEx (
new ActionEditorKeyboardKey ( name : "KeyControl" , labelText : "Shortcut:" )
. SetBehavior ( ActionEditorKeyboardKeyBehavior . KeyboardKey ));
```

### ActionEditorButton

Interactive button control for triggering actions within the editor.

```
this . ActionEditor . AddControlEx (
new ActionEditorButton ( name : "ButtonControl" , labelText : "Click me" ));
```

### ActionEditorSlider

Numeric slider control for value selection within a range.

```
this . ActionEditor . AddControlEx (
new ActionEditorSlider ( name : "SliderControl" , labelText : "Volume:" , description : "Adjust volume level" )
. SetValues ( minimumValue : 0 , maximumValue : 100 , defaultValue : 50 , step : 5 )
. SetFormatString ( "{0}%" ));
```

## Event Handling

### Control Value Changes

React to user input changes in real-time:

```
// ...
public MyActionEditorCommand ()
{
// Add controls those will trigger value change events
this . ActionEditor . AddControlEx (
new ActionEditorTextbox ( name : "TextControl" , labelText : "Enter text:" ));
this . ActionEditor . AddControlEx (
new ActionEditorButton ( name : "ButtonControl" , labelText : "Click me" ));

// Subscribe to value change events
this . ActionEditor . ControlValueChanged += this . OnControlValueChanged ;
}

private void OnControlValueChanged ( Object sender , ActionEditorControlValueChangedEventArgs e )
{
if ( e . ControlName . EqualsNoCase ( "TextControl" ))
{
var controlValue = e . ActionEditorState . GetControlValue ( "TextControl" );

// Update display name based on user input
e . ActionEditorState . SetDisplayName ( $"Action: {controlValue}" );
}
if ( e . ControlName . EqualsNoCase ( "ButtonControl" ))
{
// Handle button click
}
}
```

### Listbox Items Population

Handle dynamic population of listbox items:

```
// ...
public MyActionEditorCommand ()
{
// Add listbox controls that need dynamic population
this . ActionEditor . AddControlEx (
new ActionEditorListbox ( name : "ListControl" , labelText : "Select option:" ));

// Subscribe to event that fires when listbox needs items
this . ActionEditor . ListboxItemsRequested += this . OnListboxItemsRequested ;
}

private void OnListboxItemsRequested ( Object sender , ActionEditorListboxItemsRequestedEventArgs e )
{
if ( e . ControlName . EqualsNoCase ( "ListControl" ))
{
// Add items to the listbox
e . AddItem ( name : "option1" , displayName : "Option 1" , description : "First option" );
e . AddItem ( name : "option2" , displayName : "Option 2" , description : "Second option" );

// Optionally set default selection
e . SetSelectedItemName ( "option1" );
}
}
```

### Action Editor Lifecycle

Handle Action Editor start and finish lifecycle events:

```
// ...
public MyActionEditorCommand ()
{
// ...

// Subscribe to Action Editor lifecycle events
this . ActionEditor . Started += this . OnActionEditorStarted ;
this . ActionEditor . Finished += this . OnActionEditorFinished ;
}

private void OnActionEditorStarted ( Object sender , ActionEditorStartedEventArgs e )
{
// Called when user opens the Action Editor
// Initialize any resources, start monitoring external data, etc.
}

private void OnActionEditorFinished ( Object sender , ActionEditorFinishedEventArgs e )
{
// Called when user closes the Action Editor
// Clean up resources, stop monitoring external data, save temporary data, etc.
}
```