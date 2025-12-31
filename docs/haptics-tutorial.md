# Haptics Tutorial

## Overview

This guide will walk you through adding haptics to your plugin using the Logi Actions SDK. You'll learn how to use the Logi Plugin Tool to generate a plugin skeleton, register and trigger haptic events, debug your plugin, and package it for distribution on the Logi Marketplace. By the end of this tutorial, you'll have a working plugin that provides tactile feedback through Logitech devices with haptic support.

### Video Tutorial

## Prerequisites

Before starting this tutorial, make sure you have the required software and tools installed. See the [Prerequisites section](../Getting-started/index.html#prerequisites) in the Getting Started guide for complete setup instructions.

## Creating a Plugin Using Haptics

### Step 1: Install the Logi Plugin Tool

First, we need to install the Logi Plugin Tool, which will help us generate a plugin skeleton project.

Open a terminal in any folder and install the tool:

```
dotnet tool install --global LogiPluginTool
```

Check that the tool is installed correctly:

```
LogiPluginTool --help
```

You should see available commands, with `generate` being the one we're interested in for now.

### Step 2: Generate Your Plugin Project

Use the Logi Plugin Tool to generate a basic plugin skeleton:

```
LogiPluginTool generate Tutorial
```

This will create a new folder called `TutorialPlugin` containing a complete project structure with all necessary files.

### Step 3: Open the Project

Navigate to your new `TutorialPlugin` folder and open the solution file ( `.sln` ) in Visual Studio.

You'll be greeted with a basic project template ready for customization.

### Step 4: Configure Plugin Capabilities

Open the plugin configuration file at `src/package/metadata/LoupedeckPackage.yaml` .

This file defines your plugin's metadata, including properties such as display name, description, and version.

Find the `pluginCapabilities` section and add the `HasHapticMapping` capability:

```
pluginCapabilities :
- HasHapticMapping
```

This tells Logi Options+ that your plugin wants to utilize haptics.

**Note:** For the full plugin configuration file structure and capabilities, see [Plugin Configuration File Structure](../Plugin-structure/index.html#plugin-configuration-file-structure) .

### Step 5: Create Event Configuration Files

You need to create two new files for event configuration:

#### a) Create DefaultEventSource.yaml File

Create the default event source file at `src/package/events/DefaultEventSource.yaml` with this content:

```
events :
- name : buttonPress
displayName : Button Press
description : Triggered when the button is pressed
```

**Note:** For the full event source structure and capabilities, see [Defining Event Sources in YAML Files](../Haptics-Getting-Started/index.html#defining-event-sources-in-yaml-files) .

#### b) Create eventMapping.yaml File

The event mapping file maps your events to specific device functionality (haptics in our case).

Create the event mapping file at `src/package/events/extra/eventMapping.yaml` with the following content:

```
haptics :
buttonPress :
DEFAULT : sharp_state_change
MX Master 4 : sharp_collision # Optional: device-specific mappings
```

**Note:** The event name `buttonPress` must match exactly between these files and your code.

For the full waveform mapping structure and capabilities, see [Waveform Mapping](../Haptics-Getting-Started/index.html#waveform-mapping) .

### Step 6: Register the Event in Your Action

Open the `CounterAdjustment.cs` example action class in the `src/Actions` folder, which was generated with your project.

Now let's register and raise the event in our action code:

#### a) Override OnLoad Method

Override the `OnLoad` method to register your event.

```
protected override Boolean OnLoad ()
{
this . Plugin . PluginEvents . AddEvent (
"buttonPress" , // Event name (must match YAML files)
"Play Haptic" , // Display name
"Plays a haptic" // Description
);

return true ;
}
```

**Note:** The event name string must match exactly what you defined in `DefaultEventSource.yaml` .

#### b) Raise the Event

Raise the event in your `RunCommand` method. This is executed when the user binds and triggers your action:

```
protected override void RunCommand ( String actionParameter )
{
this . Plugin . PluginEvents . RaiseEvent (
"buttonPress" // Event name (must match YAML files)
);
}
```

### Step 7: Build and Test Your Plugin

Build your plugin in Visual Studio or use `dotnet build` in your command line. The build process will automatically create a link for Logi Options+ to load your plugin from the build directory.

After building, the `Tutorial` plugin should appear in Logi Options+ with your other installed plugins.

#### a) Rename Your Action (Optional)

To test hot reloading, we can rename the action to something more appropriate like "Trigger Haptic". After changing the name in code and rebuilding, the plugin should automatically reload and the name will update dynamically in Logi Options+.

#### b) Bind and Test

1. In Logi Options+, bind your action to any button on your device
2. Trigger the action
3. You should feel the haptic feedback on your device

If you're not feeling the haptic feedback, continue to the debugging section below.

## Debugging: Troubleshooting Haptic Events

If haptic events aren't working at this point, you should follow debug steps outlined in [Haptics Getting Started troubleshooting](../Haptics-Getting-Started/index.html#troubleshooting) .

Logging can be enabled for more detailed information. To enable logging, follow the steps outlined in [Haptics Getting Started logging](../Haptics-Getting-Started/index.html#logging) .

## Packaging and Distribution

Once your plugin is working as expected and you're ready to share it with others, you can package it for distribution as described in [Distributing the Plugin](../Distributing-the-plugin/index.html) .