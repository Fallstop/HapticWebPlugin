# Getting Started

Plugin events enable haptic feedback interactions, triggered by user actions or application state changes. A plugin or virtual device can define one or more event sources.

### Core Elements

- **Event Sources** : Collections of related events that can be triggered by your plugin
- **Event Registration** : Define event sources in [code](index.html#defining-event-sources-in-code) or in a YAML [event source definition file](index.html#defining-event-sources-in-yaml-files)
- **Event Triggering** : Raise events in [code](index.html#triggering-events)
- **Event Configuration** : Define event metadata in YAML files for enhanced functionality
- **Default event source** : The first event source is defined as the default event source

### Event Naming Rules

Event names must follow these conventions:

- First character must be a Latin letter or underscore ( `_` )
- Subsequent characters may be Latin letters, underscores, or numbers
- Name is case sensitive and must match exactly in code and YAML files ( [event source definition file](index.html#defining-event-sources-in-yaml-files) , [waveform mapping file](index.html#waveform-mapping) )
- Names must be unique within the event source

## Defining Event Sources in Code

- Defining an event source in a class inherited from the `Plugin` class `public class HapticPlugin : Plugin { private const String EventName = "periodic15min" ; public override void Load () { // Define event this . PluginEvents . AddEvent ( EventName , "Every 15 minutes" , "This haptic event is sent every 15 minutes" ); } // ... }`
- Defining an event source in action code `public class HapticDynamicCommand : PluginDynamicCommand { private const String EventName = "buttonPress" ; public HapticDynamicCommand () : base ( "Button Press" , "Invokes the haptic event on button press" , "Haptics" ) { } protected override Boolean OnLoad () { // Define event this . Plugin . PluginEvents . AddEvent ( EventName , "Button Press" , "This haptic event is sent when the user presses the button" ); return true ; } // ... }`

## Defining Event Sources in YAML Files

An event source can be defined in a YAML file that should be located in the `events` directory of the [plugin package](../Plugin-structure/index.html#events) .

The file name for the default event source should be `DefaultEventSource.yaml` .

The YAML file contains event source configuration with the following structure:

### Event Source Fields (Optional)

These fields define the event source itself:

- `name` - name of the event source. Should be missing for the default event source. Should be unique within the plugin
- `displayName` - display name of the event source. Default is plugin display name
- `description` - description of the event source. Default is plugin description
- `iconFile` - file name of event source icon. Should be located in the same directory as the YAML file. Can be in either PNG or SVG format. If icon file is missing, then the plugin icon is used

### Events Field (Required)

You can specify one or more events in the `events` field. Every event has the following fields:

- `name` - name of the event. This field is mandatory and should be unique within the event source.
- `displayName` - display name of the event. This field is mandatory.
- `description` - description of the event. This field is optional.

### Complete Example

```
displayName : My application
description : Contains various events generated for the application
iconFile : DefaultEventSource.svg

events :
- name : periodic15min
displayName : Every 15 minutes
description : This haptic event is sent every 15 minutes
- name : buttonPress
displayName : Button Press
description : This haptic event is sent when the user presses the button
```

### Non-Default Event Sources

For non-default event sources, all the fields mentioned in the [Event Source Fields](index.html#event-source-fields-optional) section above should be specified.

`name` field value cannot be `Default` , as this value is reserved for the default event source.

## Waveform Mapping

Define haptic waveform mappings in `/events/extra/eventMapping.yaml` :

```
haptics :
periodic15min :
DEFAULT : happy_alert
buttonPress :
DEFAULT : sharp_state_change
MX Master 4 : sharp_collision # Device-specific mapping
```

The `DEFAULT` key specifies the fallback waveform if no device-specific mapping is found.

See [Waveforms](index.html#waveforms) for more information.

## Plugin Configuration for Haptics

To enable haptic functionality in your plugin, you must declare the `HasHapticMapping` capability in your plugin configuration file.

### Adding HasHapticMapping Capability

Add the `HasHapticMapping` capability to the `pluginCapabilities` field in your `LoupedeckPackage.yaml` file:

```
# ... other configuration fields

pluginCapabilities :
- HasHapticMapping # Enables haptics

# ... other configuration fields
```

See [Plugin Capabilities](../Plugin-Capabilities/index.html#plugin-configuration-file) and [Plugin Structure](../Plugin-structure/index.html#plugin-configuration-file-structure) for more information about plugin capabilities.

## Triggering Events

- Triggering a defined event in a class inherited from the `Plugin` class `public class HapticPlugin : Plugin { private const String EventName = "periodic15min" ; private readonly System . Timers . Timer _periodicEventTimer = new (); public override void Load () { this . PluginEvents . AddEvent ( EventName , "Every 15 minutes" , "This haptic event is sent every 15 minutes" ); this . _periodicEventTimer . AutoReset = true ; this . _periodicEventTimer . Interval = 900000 ; this . _periodicEventTimer . Elapsed += this . OnPeriodicEventTimerElapsed ; this . _periodicEventTimer . Start (); } public override void Unload () { this . _periodicEventTimer . Stop (); this . _periodicEventTimer . Elapsed -= this . OnPeriodicEventTimerElapsed ; } private void OnPeriodicEventTimerElapsed ( Object sender , System . Timers . ElapsedEventArgs e ) { // Trigger event this . PluginEvents . RaiseEvent ( EventName ); } }`
- Triggering a defined event in action code `public class HapticDynamicCommand : PluginDynamicCommand { private const String EventName = "buttonPress" ; public HapticDynamicCommand () : base ( "Button Press" , "Invokes the haptic event on button press" , "Haptics" ) { } protected override Boolean OnLoad () { this . Plugin . PluginEvents . AddEvent ( EventName , "Button Press" , "This haptic event is sent when the user presses the button" ); return true ; } protected override void RunCommand ( String actionParameter ) { // Trigger event this . Plugin . PluginEvents . RaiseEvent ( EventName ); } }`

## Waveforms

### Waveform Groups

The following waveforms are available for haptic events:

**State Change Waveforms** :

- `sharp_state_change` : Short, high-intensity pulse for discrete state transitions (button presses, toggles)
- `damp_state_change` : Gradual intensity change for smooth state transitions

**Collision Waveforms** :

- `sharp_collision` : High-intensity impact simulation for collision events
- `damp_collision` : Medium-intensity impact with gradual decay
- `subtle_collision` : Low-intensity feedback for light contact events

**Alert Waveforms** :

- `happy_alert` : Positive feedback pattern for success states
- `angry_alert` : Attention-grabbing pattern for error conditions
- `completed` : Confirmation pattern for task completion

**Special Waveforms** :

- `square` : Sharp-edged waveform with defined start/stop points
- `wave` : Smooth sinusoidal pattern with gradual transitions
- `firework` : Multi-burst pattern with varying intensities
- `mad` : High-frequency chaotic pattern
- `knock` : Repetitive impact pattern
- `jingle` : Musical-style pattern with multiple tones
- `ringing` : Continuous oscillating pattern

### Available Waveforms

The available waveforms offer a variety of haptic sensations to enhance user experiences. From textures to dynamic feedback, these waveforms allow developers to create immersive and engaging interactions tailored to their applications.

Waveform usage falls into three distinct categories, determined by the type of event that triggers them.

| Precision enhancers  (feedback on physical interaction between digital elements)  [See examples](../Haptics-Best-Practices/index.html#precision-enhancers)   | Progress indicators  (gently inform about a starting, ending or advancing process)  [See examples](../Haptics-Best-Practices/index.html#progress-indicators)   | Incoming events  (grab attention toward a new event or status)  [See examples](../Haptics-Best-Practices/index.html#incoming-events)   |
|--------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------|
| Sharp Collision waveform  <!-- image -->                                                                                                                     | Sharp State Change waveform  <!-- image -->                                                                                                                    | Knock waveform  <!-- image -->  Knock  Incoming events                                                                                 |
| Damp Collision waveform  <!-- image -->  Damp Collision  Precision enhancer                                                                                  | Mad waveform  <!-- image -->  Mad  Progress indicator                                                                                                          | Ringing waveform  <!-- image -->  Ringing  Incoming events                                                                             |
| Subtle Collision waveform  <!-- image -->  Subtle Collision  Precision enhancer                                                                              | Completed waveform  <!-- image -->  Completed  Progress indicator                                                                                              | Jingle waveform  <!-- image -->  Jingle  Incoming events                                                                               |
| Damp State Change waveform  <!-- image -->  Damp State Change  Precision enhancer                                                                            | Firework waveform  <!-- image -->  Firework  Progress indicator                                                                                                |                                                                                                                                        |
|                                                                                                                                                              | Happy Alert waveform  <!-- image -->  Happy Alert  Progress indicator  Incoming events                                                                         |                                                                                                                                        |
|                                                                                                                                                              | Wave waveform  <!-- image -->  Wave  Progress indicator  Incoming events                                                                                       |                                                                                                                                        |
|                                                                                                                                                              | Angry Alert waveform  <!-- image -->  Angry Alert  Progress indicator                                                                                          |                                                                                                                                        |
|                                                                                                                                                              | Square waveform  <!-- image -->  Square  Progress indicator                                                                                                    |                                                                                                                                        |

<!-- image -->

<!-- image -->

<!-- image -->

<!-- image -->

<!-- image -->

<!-- image -->

<!-- image -->

<!-- image -->

<!-- image -->

<!-- image -->

<!-- image -->

<!-- image -->

<!-- image -->

<!-- image -->

<!-- image -->

## Logging

To enable logs, see [Logi Plugin Service Logging](../Logging/index.html#logi-plugin-service-logging) .

Log files are located in the `Logs` subdirectory of the Logi Plugin Service data directory:

- **Windows** :
    - C:\Users\&lt;user\_name&gt;\AppData\Local\Logi\LogiPluginService\Logs\messages\eventSources
    - C:\Users\&lt;user\_name&gt;\AppData\Local\Logi\LogiPluginService\Logs\testing\event\_sources.txt
- **macOS** :
    - ~/Users/&lt;user\_name&gt;/Library/Application Support/Logi/LogiPluginService/Logs/messages/eventSources
    - ~/Users/&lt;user\_name&gt;/Library/Application Support/Logi/LogiPluginService/Logs/testing/event\_sources.txt

## Troubleshooting

### Common Issues

- **Event Not Triggering**
    - Verify event registration and raising
    - Check event name consistency between code and YAML files
    - Ensure plugin is properly loaded (check logs)
    - Verify event appears in the event logs
- **Event Source Not Found**
    - Check that YAML files are correctly placed in `events` directory
    - Ensure file naming follows conventions (e.g., default event source file name should be `DefaultEventSource.yaml` )
    - Verify YAML syntax is valid
- **Event Name Conflicts**
    - Ensure event names are unique within the event source
    - Verify [event naming rules](index.html#event-naming-rules) are followed
- **No Haptic Feedback**
    - Verify that the haptic device is shown in Logi Options+
    - Check that the `/events/extra/eventMapping.yaml` file exists and contains haptic mappings
    - Ensure the haptic device is properly connected and recognized
    - Verify device supports haptic feedback (see [Supported Devices](../Haptics-Overview/index.html#supported-devices) )
- **Incorrect Waveform**
    - Verify the waveform name is correctly spelled in the `/events/extra/eventMapping.yaml` file
    - Check the device-specific mappings are properly configured
    - Ensure `DEFAULT` mapping is present as fallback in the `/events/extra/eventMapping.yaml` file
    - Confirm the waveform exists in [Waveform Groups](index.html#waveform-groups)
- **Weak or Inconsistent Haptic Response**
    - Try different waveforms to find optimal feedback for your use case
    - Check device-specific mappings for better hardware optimization
    - Ensure device battery level is sufficient for haptic feedback