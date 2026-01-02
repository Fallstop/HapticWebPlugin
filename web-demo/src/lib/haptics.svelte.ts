import { WAVEFORMS, type Waveform } from './haptics';
import {
    HapticsWebSocket,
    type ConnectionState,
    type ErrorType,
    type HapticsWebSocketState
} from './haptics-ws';

// Svelte 5 reactive state for selected waveform
let selectedWaveformApiName = $state<string>('sharp_collision');

// Svelte 5 reactive state for WebSocket connection
let wsState = $state<HapticsWebSocketState>({
    connectionState: 'disconnected',
    errorType: 'none',
    errorMessage: null,
});

// WebSocket client instance
let wsClient: HapticsWebSocket | null = null;

export function getSelectedWaveform(): Waveform {
    return WAVEFORMS.find(w => w.api_name === selectedWaveformApiName) || WAVEFORMS[0];
}

export function getSelectedWaveformApiName(): string {
    return selectedWaveformApiName;
}

export function setSelectedWaveform(apiName: string): void {
    selectedWaveformApiName = apiName;
}

/** Get current WebSocket connection state (reactive) */
export function getConnectionState(): ConnectionState {
    return wsState.connectionState;
}

/** Get current error type (reactive) */
export function getErrorType(): ErrorType {
    return wsState.errorType;
}

/** Get current error message (reactive) */
export function getErrorMessage(): string | null {
    return wsState.errorMessage;
}

/** Check if connected (reactive) */
export function isConnected(): boolean {
    return wsState.connectionState === 'connected';
}

/** Initialize and connect the WebSocket client */
export function initHapticsWebSocket(): void {
    if (wsClient) return;

    wsClient = new HapticsWebSocket({
        onStateChange: (connectionState, errorType, errorMessage) => {
            wsState = { connectionState, errorType, errorMessage };
        },
    });

    wsClient.connect();
}

/** Disconnect the WebSocket client */
export function disconnectHapticsWebSocket(): void {
    wsClient?.disconnect();
}

/** Destroy the WebSocket client */
export function destroyHapticsWebSocket(): void {
    wsClient?.destroy();
    wsClient = null;
}

/** Trigger haptic by waveform API name using WebSocket */
export function triggerHapticWs(apiName: string): boolean {
    if (!wsClient) return false;

    const index = WAVEFORMS.findIndex(w => w.api_name === apiName);
    if (index === -1) return false;

    return wsClient.triggerHaptic(index);
}

/** Trigger haptic for currently selected waveform using WebSocket */
export function triggerSelectedHapticWs(): boolean {
    return triggerHapticWs(selectedWaveformApiName);
}