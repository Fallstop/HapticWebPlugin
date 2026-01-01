# HapticWebPlugin

A Logi Actions SDK plugin that exposes MX Master 4 haptic feedback via a local HTTP API, enabling external programs to trigger tactile waveforms.

## Requirements

- Logitech MX Master 4 mouse
- Logi Options+ installed
- Logi Plugin Service running

## API Server

The plugin runs an HTTP server on `http://127.0.0.1:8765/` when loaded.

### Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/` | GET | Health check with service info and available endpoints |
| `/waveforms` | GET | List all 16 available haptic **waveforms** with descriptions |
| `/haptic/{waveform}` | POST | Trigger a specific haptic waveform |

### Available Waveforms

| Waveform | Description |
|----------|-------------|
| `sharp_state_change` | Short, high-intensity pulse for discrete state transitions |
| `damp_state_change` | Gradual intensity change for smooth state transitions |
| `sharp_collision` | High-intensity impact simulation for collision events |
| `damp_collision` | Medium-intensity impact with gradual decay |
| `subtle_collision` | Low-intensity feedback for light contact events |
| `happy_alert` | Positive feedback pattern for success states |
| `angry_alert` | Attention-grabbing pattern for error conditions |
| `completed` | Confirmation pattern for task completion |
| `square` | Sharp-edged waveform with defined start/stop points |
| `wave` | Smooth sinusoidal pattern with gradual transitions |
| `firework` | Multi-burst pattern with varying intensities |
| `mad` | High-frequency chaotic pattern |
| `knock` | Repetitive impact pattern |
| `jingle` | Musical-style pattern with multiple tones |
| `ringing` | Continuous oscillating pattern |
| `heartbeat` | Rhythmic double-pulse pattern |

## Example Usage

```bash
# Health check
curl http://127.0.0.1:8765/

# List all available waveforms
curl http://127.0.0.1:8765/waveforms

# Trigger haptic feedback
curl -X POST http://127.0.0.1:8765/haptic/sharp_collision
curl -X POST http://127.0.0.1:8765/haptic/happy_alert
curl -X POST http://127.0.0.1:8765/haptic/completed
```

### Example Response

```json
{
  "success": true,
  "waveform": "sharp_collision",
  "deviceStatus": "unknown",
  "note": "Haptic event raised. Will trigger if MX Master 4 is connected."
}
```

## Device Status

The API reports `"deviceStatus": "unknown"` because the Logi Actions SDK does not provide a way to detect if the MX Master 4 is connected. Haptic events are sent regardless—they will trigger if the device is connected and be silently ignored otherwise.

## Building

```bash
dotnet build -c Debug
```

The plugin auto-reloads after building if the Logi Plugin Service is running.
