# Multistate Plugin Actions

By default, plugin actions have one state.

The plugin can define more than one state for any dynamic action.

States are identified by a 0-based index.

Once set, the number of states cannot be changed.

Each state has its own:

- display name;
- description;
- button image;
- LED color.

## Plugin API

### Dynamic command

Create a class inherited from `PluginMultistateDynamicCommand` abstract class.

### Define multiple states

```
protected Int32 AddState ( String displayName , String description )
```

In the dynamic action class constructor, the plugin calls `AddState()` method for every action state.

E.g. if the command has "on" and "off" states:

```
public LampSwitchDynamicCommand ()
{
this . AddState ( "On" , "Lamp is turned on" );
this . AddState ( "Off" , "Lamp is turned off" );
}
```

### Get action states

```
public IReadOnlyList < PluginMultistateDynamicCommandState > States { get ; }
```

Is `null` by default and is created during the first call of the `AddState()` method.

### Get current action state

All methods that work with the current state have two overloads: without and with the `actionParameter` parameter.

```
public Boolean TryGetCurrentState ( out Int32 currentState );
public Boolean TryGetCurrentState ( String actionParameter , out Int32 currentState );
```

### Change the current action state

Plugin calls `SetCurrentState()` method to change the current state.

```
protected void SetCurrentState ( Int32 newStateIndex );
protected void SetCurrentState ( String actionParameter , Int32 newStateIndex );
```

### Increment and decrement current action state

```
protected void IncrementCurrentState ( String actionParameter );
protected void IncrementCurrentState ();

protected void DecrementCurrentState ( String actionParameter );
protected void DecrementCurrentState ();
```

The plugin calls these methods to increment and decrement the current state.

Incrementing the last state activates the first state. Decrementing the first state activates the last state.

### Toggle the current action state

```
protected void ToggleCurrentState ( String actionParameter );
protected void ToggleCurrentState ();
```

Plugin calls `ToggleCurrentState()` method to toggle the current state.

Works only actions with 2 states. In other cases throws `InvalidOperationException` exception.

E.g. if the command has "on" and "off" states:

```
protected override void RunCommand ( String actionParameter ) => this . ToggleCurrentState ( actionParameter );
```

### Get the state display name

```
protected virtual String GetCommandDisplayName ( String actionParameter , Int32 deviceState , PluginImageSize imageSize );
```

Overload `GetCommandDisplayName` method with an additional `deviceState` parameter is called for actions with more than one state.

### Get state image

```
protected virtual BitmapImage GetCommandImage ( String actionParameter , Int32 deviceState , PluginImageSize imageSize );
```

Overload `GetCommandImage` method with an additional `deviceState` parameter is called for actions with more than one state.

## Minimal example

```
namespace Loupedeck.Test4Plugin
{
using System ;

public class ToggleMultistateDynamicCommand : PluginMultistateDynamicCommand
{
public ToggleMultistateDynamicCommand ()
: base ( "Toggle Multistate" , null , "Test" )
{
this . AddState ( "On" , "Turn me on" );
this . AddState ( "Off" , "Turn me off" );
}

protected override void RunCommand ( String actionParameter ) => this . ToggleCurrentState ();
}
}
```