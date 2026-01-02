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

| Waveform | Category |
|----------|----------|
| `sharp_collision` | Precision enhancers |
| `sharp_state_change` | Progress indicators |
| `knock` | Incoming events |
| `damp_collision` | Precision enhancers |
| `mad` | Progress indicators |
| `ringing` | Incoming events |
| `subtle_collision` | Precision enhancers |
| `completed` | Progress indicators |
| `jingle` | Incoming events |
| `damp_state_change` | Precision enhancers |
| `firework` | Progress indicators |
| `happy_alert` | Progress indicators |
| `wave` | Progress indicators |
| `angry_alert` | Progress indicators |
| `square` | Progress indicators |

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
  "waveform": "sharp_collision"
}
```

## Building

```bash
dotnet build -c Debug
```

The plugin auto-reloads after building if the Logi Plugin Service is running.
