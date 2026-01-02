/**
 * HapticsWebSocket - Framework-agnostic WebSocket client for low-latency haptic triggering
 * 
 * Features:
 * - Auto-reconnect with exponential backoff (capped at 15s)
 * - Pre-allocated buffer for zero-allocation sends
 * - Service validation to detect wrong service on port
 * - Graceful error handling for connection refused, timeout, wrong service
 */

import { API_BASE_URL } from './config';

export type ConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

export type ErrorType =
    | 'none'
    | 'connection_refused'      // Plugin not installed or not running
    | 'timeout'                 // Connection timed out
    | 'wrong_service'           // Different service on the port
    | 'websocket_error'         // Generic WebSocket error
    | 'ssl_error';              // Certificate issue

export interface HapticsWebSocketOptions {
    /** Base URL for the haptics service (default: 'https://local.jmw.nz:41443') */
    baseUrl?: string;
    /** Initial reconnect delay in ms (default: 1000) */
    initialReconnectDelay?: number;
    /** Maximum reconnect delay in ms (default: 15000) */
    maxReconnectDelay?: number;
    /** Connection timeout in ms (default: 5000) */
    connectionTimeout?: number;
    /** Callback when connection state changes */
    onStateChange?: (state: ConnectionState, errorType: ErrorType, errorMessage: string | null) => void;
}

export interface HapticsWebSocketState {
    connectionState: ConnectionState;
    errorType: ErrorType;
    errorMessage: string | null;
}

const DEFAULT_OPTIONS: Required<Omit<HapticsWebSocketOptions, 'onStateChange'>> = {
    baseUrl: API_BASE_URL,
    initialReconnectDelay: 1000,
    maxReconnectDelay: 15000,
    connectionTimeout: 5000,
};

export class HapticsWebSocket {
    private options: Required<Omit<HapticsWebSocketOptions, 'onStateChange'>> & Pick<HapticsWebSocketOptions, 'onStateChange'>;
    private ws: WebSocket | null = null;
    private reconnectTimer: ReturnType<typeof setTimeout> | null = null;
    private connectionTimer: ReturnType<typeof setTimeout> | null = null;
    private currentReconnectDelay: number;
    private isDestroyed = false;

    // Pre-allocated buffer for zero-allocation sends
    private readonly sendBuffer = new Uint8Array(1);

    // Current state
    private _state: HapticsWebSocketState = {
        connectionState: 'disconnected',
        errorType: 'none',
        errorMessage: null,
    };

    constructor(options: HapticsWebSocketOptions = {}) {
        this.options = { ...DEFAULT_OPTIONS, ...options };
        this.currentReconnectDelay = this.options.initialReconnectDelay;
    }

    /** Get current connection state */
    get state(): Readonly<HapticsWebSocketState> {
        return this._state;
    }

    /** Start connection (will auto-reconnect on failure) */
    connect(): void {
        if (this.isDestroyed) return;
        this.attemptConnection();
    }

    /** Disconnect and stop auto-reconnect */
    disconnect(): void {
        this.clearTimers();
        if (this.ws) {
            this.ws.onclose = null; // Prevent reconnect on intentional close
            this.ws.close();
            this.ws = null;
        }
        this.updateState('disconnected', 'none', null);
    }

    /** Permanently destroy the client */
    destroy(): void {
        this.isDestroyed = true;
        this.disconnect();
    }

    /**
     * Trigger a haptic effect by waveform index
     * Uses pre-allocated buffer for zero GC pressure
     */
    triggerHaptic(index: number): boolean {
        if (!this.ws || this.ws.readyState !== WebSocket.OPEN) {
            return false;
        }

        this.sendBuffer[0] = index & 0xFF;
        this.ws.send(this.sendBuffer);
        return true;
    }

    private async attemptConnection(): Promise<void> {
        if (this.isDestroyed) return;

        this.clearTimers();
        this.updateState(
            this._state.connectionState === 'disconnected' ? 'connecting' : 'reconnecting',
            'none',
            null
        );

        // First, validate the service is correct by checking the health endpoint
        const validation = await this.validateService();
        if (!validation.valid) {
            this.updateState('disconnected', validation.errorType, validation.errorMessage);
            this.scheduleReconnect();
            return;
        }

        // Now attempt WebSocket connection
        const wsUrl = this.options.baseUrl.replace(/^http/, 'ws') + '/ws';

        try {
            this.ws = new WebSocket(wsUrl);
            this.ws.binaryType = 'arraybuffer';

            // Set connection timeout
            this.connectionTimer = setTimeout(() => {
                if (this.ws && this.ws.readyState === WebSocket.CONNECTING) {
                    this.ws.close();
                    this.updateState('disconnected', 'timeout', 'Connection timed out');
                    this.scheduleReconnect();
                }
            }, this.options.connectionTimeout);

            this.ws.onopen = () => {
                this.clearTimers();
                this.currentReconnectDelay = this.options.initialReconnectDelay; // Reset backoff
                this.updateState('connected', 'none', null);
            };

            this.ws.onclose = (event) => {
                this.clearTimers();
                if (!this.isDestroyed) {
                    const errorMessage = event.reason || 'Connection closed';
                    this.updateState('disconnected', 'websocket_error', errorMessage);
                    this.scheduleReconnect();
                }
            };

            this.ws.onerror = () => {
                // Error details are limited in browser for security
                // The onclose handler will fire after this
            };

        } catch (error) {
            this.updateState('disconnected', 'websocket_error', String(error));
            this.scheduleReconnect();
        }
    }

    private async validateService(): Promise<{ valid: boolean; errorType: ErrorType; errorMessage: string | null }> {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), this.options.connectionTimeout);

        try {
            const response = await fetch(this.options.baseUrl + '/', {
                method: 'GET',
                signal: controller.signal,
            });

            clearTimeout(timeoutId);

            if (!response.ok) {
                return { valid: false, errorType: 'wrong_service', errorMessage: `Unexpected status: ${response.status}` };
            }

            const data = await response.json();

            if (data.service !== 'HapticWebPlugin') {
                return {
                    valid: false,
                    errorType: 'wrong_service',
                    errorMessage: `Wrong service on port: ${data.service || 'unknown'}`
                };
            }

            return { valid: true, errorType: 'none', errorMessage: null };

        } catch (error) {
            clearTimeout(timeoutId);

            if (error instanceof Error) {
                if (error.name === 'AbortError') {
                    return { valid: false, errorType: 'timeout', errorMessage: 'Health check timed out' };
                }

                // Check for SSL/certificate errors
                if (error.message.includes('SSL') || error.message.includes('certificate') || error.message.includes('CERT')) {
                    return { valid: false, errorType: 'ssl_error', errorMessage: 'SSL certificate error - you may need to trust the certificate' };
                }

                // Connection refused or network error
                if (error.message.includes('Failed to fetch') || error.message.includes('NetworkError')) {
                    return { valid: false, errorType: 'connection_refused', errorMessage: 'Plugin not running or not installed' };
                }
            }

            return { valid: false, errorType: 'connection_refused', errorMessage: 'Cannot connect to haptics service' };
        }
    }

    private scheduleReconnect(): void {
        if (this.isDestroyed || this.reconnectTimer) return;

        this.reconnectTimer = setTimeout(() => {
            this.reconnectTimer = null;
            this.attemptConnection();
        }, this.currentReconnectDelay);

        // Exponential backoff
        this.currentReconnectDelay = Math.min(
            this.currentReconnectDelay * 2,
            this.options.maxReconnectDelay
        );
    }

    private clearTimers(): void {
        if (this.reconnectTimer) {
            clearTimeout(this.reconnectTimer);
            this.reconnectTimer = null;
        }
        if (this.connectionTimer) {
            clearTimeout(this.connectionTimer);
            this.connectionTimer = null;
        }
    }

    private updateState(connectionState: ConnectionState, errorType: ErrorType, errorMessage: string | null): void {
        this._state = { connectionState, errorType, errorMessage };
        this.options.onStateChange?.(connectionState, errorType, errorMessage);
    }
}

// Singleton instance for convenience
let defaultInstance: HapticsWebSocket | null = null;

export function getHapticsWebSocket(options?: HapticsWebSocketOptions): HapticsWebSocket {
    if (!defaultInstance) {
        defaultInstance = new HapticsWebSocket(options);
    }
    return defaultInstance;
}

export function triggerHaptic(index: number): boolean {
    return defaultInstance?.triggerHaptic(index) ?? false;
}

export function connectHaptics(options?: HapticsWebSocketOptions): void {
    getHapticsWebSocket(options).connect();
}

export function disconnectHaptics(): void {
    defaultInstance?.disconnect();
}
