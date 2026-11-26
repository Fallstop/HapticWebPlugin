/**
 * HapticsWebSocket - Framework-agnostic WebSocket client for low-latency haptic triggering
 * 
 * Features:
 * - Auto-reconnect with exponential backoff (capped at 15s)
 * - Pre-allocated buffer for zero-allocation sends
 * - Service validation to detect wrong service on port
 * - Tries plain ws://127.0.0.1 first, then the HTTPS fallback (for Safari)
 * - Graceful error handling for connection refused, timeout, wrong service
 */

import { API_BASE_URLS } from './config';

export type ConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

export type ErrorType =
    | 'none'
    | 'connection_refused'      // Plugin not installed or not running
    | 'timeout'                 // Connection timed out
    | 'wrong_service'           // Different service on the port
    | 'websocket_error'         // Generic WebSocket error
    | 'ssl_error';              // Certificate issue

type ServiceFailure = { valid: false; errorType: ErrorType; errorMessage: string | null };

export interface HapticsWebSocketOptions {
    /**
     * Base URLs for the haptics service, tried in order
     * (default: ['http://127.0.0.1:41080', 'https://local.jmw.nz:41443'])
     */
    baseUrls?: string[];
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
    baseUrls: API_BASE_URLS,
    initialReconnectDelay: 1000,
    maxReconnectDelay: 15000,
    connectionTimeout: 5000,
};

export class HapticsWebSocket {
    private options: Required<Omit<HapticsWebSocketOptions, 'onStateChange'>> & Pick<HapticsWebSocketOptions, 'onStateChange'>;
    private ws: WebSocket | null = null;
    private reconnectTimer: ReturnType<typeof setTimeout> | null = null;
    // Incremented by every attempt and by disconnect(), so a stale attempt can tell it should stop
    private attemptId = 0;
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
        this.attemptId++;
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
        const attempt = ++this.attemptId;
        this.updateState(
            this._state.connectionState === 'disconnected' ? 'connecting' : 'reconnecting',
            'none',
            null
        );

        // Try each endpoint in order: it must answer the health check AND accept the WebSocket,
        // since a browser can allow one and block the other.
        let failure: ServiceFailure | null = null;
        for (const baseUrl of this.options.baseUrls) {
            const validation = await this.validateService(baseUrl);
            if (attempt !== this.attemptId) return; // disconnected or superseded meanwhile

            const result = validation.valid
                ? await this.openWebSocket(baseUrl.replace(/^http/, 'ws') + '/ws')
                : validation;
            if (attempt !== this.attemptId) {
                if ('ws' in result) result.ws.close();
                return;
            }

            if ('ws' in result) {
                this.adopt(result.ws);
                return;
            }

            // Report the most specific error (anything over "connection refused")
            if (!failure || failure.errorType === 'connection_refused') {
                failure = result;
            }
        }

        failure ??= { valid: false, errorType: 'connection_refused', errorMessage: 'Cannot connect to haptics service' };
        this.updateState('disconnected', failure.errorType, failure.errorMessage);
        this.scheduleReconnect();
    }

    /** Resolves with the open socket, or with why it failed to open. */
    private openWebSocket(url: string): Promise<{ ws: WebSocket } | ServiceFailure> {
        return new Promise((resolve) => {
            let ws: WebSocket;
            try {
                ws = new WebSocket(url);
            } catch (error) {
                resolve({ valid: false, errorType: 'websocket_error', errorMessage: String(error) });
                return;
            }
            ws.binaryType = 'arraybuffer';

            const timer = setTimeout(() => {
                ws.onopen = ws.onclose = null;
                ws.close();
                resolve({ valid: false, errorType: 'timeout', errorMessage: 'Connection timed out' });
            }, this.options.connectionTimeout);

            ws.onopen = () => {
                clearTimeout(timer);
                ws.onopen = ws.onclose = null;
                resolve({ ws });
            };

            // Error details are limited in browser for security; onclose always follows onerror
            ws.onclose = (event) => {
                clearTimeout(timer);
                resolve({ valid: false, errorType: 'websocket_error', errorMessage: event.reason || 'WebSocket connection failed' });
            };
        });
    }

    private adopt(ws: WebSocket): void {
        this.ws = ws;
        this.currentReconnectDelay = this.options.initialReconnectDelay; // Reset backoff

        ws.onclose = (event) => {
            this.ws = null;
            if (!this.isDestroyed) {
                this.updateState('disconnected', 'websocket_error', event.reason || 'Connection closed');
                this.scheduleReconnect();
            }
        };

        this.updateState('connected', 'none', null);
    }

    private async validateService(baseUrl: string): Promise<ServiceFailure | { valid: true }> {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), this.options.connectionTimeout);

        try {
            const response = await fetch(baseUrl + '/', {
                method: 'GET',
                signal: controller.signal,
            });

            if (!response.ok) {
                return { valid: false, errorType: 'wrong_service', errorMessage: `Unexpected status: ${response.status}` };
            }

            const data = await response.json();
            clearTimeout(timeoutId);

            if (data.service !== 'HapticWebPlugin') {
                return {
                    valid: false,
                    errorType: 'wrong_service',
                    errorMessage: `Wrong service on port: ${data.service || 'unknown'}`
                };
            }

            return { valid: true };

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
