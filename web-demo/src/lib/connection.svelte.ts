/**
 * Global connection store that persists across navigation.
 * Connection only starts when user explicitly calls connect().
 * Remembers if user has connected before and auto-reconnects on reload.
 */

import { browser } from '$app/environment';
import { WAVEFORMS, type Waveform } from './haptics';
import {
    HapticsWebSocket,
    type ConnectionState,
    type ErrorType,
    type HapticsWebSocketState
} from './haptics-ws';

const STORAGE_KEY = 'hapticweb_has_connected';

// Connection state - starts disconnected, no auto-connect
let wsState = $state<HapticsWebSocketState>({
    connectionState: 'disconnected',
    errorType: 'none',
    errorMessage: null,
});

// Track if user has ever initiated a connection this session
let hasUserInitiatedConnection = $state(false);

// Selected waveform for demos
let selectedWaveformApiName = $state<string>('sharp_collision');

// Singleton WebSocket client
let wsClient: HapticsWebSocket | null = null;

// Check if we should auto-reconnect on load
function shouldAutoReconnect(): boolean {
    if (!browser) return false;
    return localStorage.getItem(STORAGE_KEY) === 'true';
}

// Remember that we've connected
function rememberConnection(): void {
    if (!browser) return;
    localStorage.setItem(STORAGE_KEY, 'true');
}

// Auto-reconnect if previously connected
if (browser && shouldAutoReconnect()) {
    // Delay slightly to avoid blocking initial render
    setTimeout(() => {
        connect(false); // Don't play feedback on auto-reconnect
    }, 100);
}

// --- Waveform selection ---

export function getSelectedWaveform(): Waveform {
    return WAVEFORMS.find(w => w.api_name === selectedWaveformApiName) || WAVEFORMS[0];
}

export function getSelectedWaveformApiName(): string {
    return selectedWaveformApiName;
}

export function setSelectedWaveform(apiName: string): void {
    selectedWaveformApiName = apiName;
}

// --- Connection state (reactive) ---

export function getConnectionState(): ConnectionState {
    return wsState.connectionState;
}

export function getErrorType(): ErrorType {
    return wsState.errorType;
}

export function getErrorMessage(): string | null {
    return wsState.errorMessage;
}

export function isConnected(): boolean {
    return wsState.connectionState === 'connected';
}

export function hasInitiatedConnection(): boolean {
    return hasUserInitiatedConnection;
}

// --- Connection control ---

/**
 * Initiate connection to the haptics service.
 * Call this only in response to user action (button click).
 * @param playFeedback - If true, plays a tap haptic when connected (default: true for user-initiated)
 */
export function connect(playFeedback: boolean = true): void {
    if (wsClient) return;

    hasUserInitiatedConnection = true;
    rememberConnection();

    wsClient = new HapticsWebSocket({
        onStateChange: (connectionState, errorType, errorMessage) => {
            const wasConnecting = wsState.connectionState === 'connecting';
            wsState = { connectionState, errorType, errorMessage };

            // Play feedback haptic when connection succeeds
            if (playFeedback && wasConnecting && connectionState === 'connected') {
                setTimeout(() => triggerHaptic('jingle'), 50);
            }
        },
    });

    wsClient.connect();
}

/**
 * Disconnect from the haptics service.
 */
export function disconnect(): void {
    wsClient?.disconnect();
}

/**
 * Destroy the connection entirely.
 * Call this when leaving the app.
 */
export function destroy(): void {
    wsClient?.destroy();
    wsClient = null;
    hasUserInitiatedConnection = false;
}

// --- Haptic triggers ---

/**
 * Trigger haptic by waveform API name.
 */
export function triggerHaptic(apiName: string): boolean {
    if (!wsClient) return false;

    const index = WAVEFORMS.findIndex(w => w.api_name === apiName);
    if (index === -1) return false;

    return wsClient.triggerHaptic(index);
}

/**
 * Trigger the currently selected waveform.
 */
export function triggerSelectedHaptic(): boolean {
    return triggerHaptic(selectedWaveformApiName);
}
