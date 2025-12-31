# Best Practices

## Example Use Cases

There are two primary types of use cases when designing haptic experiences: Feedback and Notifications.

### Feedback

Haptics provide immediate tactile confirmation of user actions, helping users to build muscle memory, speed up reaction times, and feel more precise.

Feedback haptics use case

<!-- image -->

Trigger haptic feedback when the cursor or an object spatially snaps with:

- Alignment guides
- Important vertices &amp; handles
- Artboard edges
- Timeline markers

Trigger haptic feedback to highlight when there is a change in the click input context:

- More options available
- Change in the type of selection
- Different manipulation for the selected element

### Notifications

Turn your sound off and stay longer in your flow state without missing gentle reminders being directed to the haptic button.

Notification haptics use case

<!-- image -->

Trigger haptic notification to know when:

- A long process was completed
- An event is happening in the background
- Code is compiled

## Design Guidelines

### Waveform Selection

- Use subtle waveforms for frequent events
- Reserve intense waveforms for important notifications
- Consider the context of your application/plugin

### Event Timing

- Avoid triggering haptic events too frequently
- Ensure haptic feedback aligns with visual feedback and plugin functionality

### Device Support

- Always provide a `DEFAULT` waveform
- Test on supported devices when possible

## Examples

### Precision Enhancers

Event: Cursor hovers on Actions Ring Element Haptic feedback: The waveform Subtle Collision is played

<!-- image -->

Event: Playhead snaps with beginning of clip Haptic feedback: The waveform Subtle Collision is played

<!-- image -->

Event: Layer content snap with smart guide Haptic feedback: The waveform Subtle Collision is played

<!-- image -->

Event: Handle collides with end of slider Haptic feedback: The waveform Subtle Collision is played

<!-- image -->

Event: Cropped area reached max height/width Haptic feedback: The waveform Subtle Collision is played

<!-- image -->

### Progress Indicators

Event: Adobe Premiere Pro export progress bar is full Haptic feedback: The waveform Subtle Collision is played

<!-- image -->

Event: AI Mask was created Haptic feedback: The waveform Damp State Change is played

<!-- image -->

### Incoming Events

Event: Someone is calling you Haptic feedback: The waveform Ringing is played

<!-- image -->

Event: Someone entered the waiting room Haptic feedback: The waveform Knock is played

<!-- image -->

Event: Unable to join this meeting Haptic feedback: The waveform Mad is played

<!-- image -->