const API_BASE = 'https://local.jmw.nz:41443';

import waveformsData from '$lib/data/waveforms.json';

export interface WaveformEncodingItem {
    type: 'bar' | 'dot';
    haptic_type: 'knock' | 'vibration' | 'silent';
    strength?: 1 | 2 | 3;
}

export interface Waveform {
    name: string;
    category: string;
    tags: string[];
    waveform_encoding: WaveformEncodingItem[];
    api_name: string; // snake_case version for API calls
}

// Load waveforms from JSON
export const WAVEFORMS: Waveform[] = waveformsData as Waveform[];

export const CATEGORY_INFO: Record<string, { color: string; description: string }> = {
    'Precision enhancers': {
        color: 'bg-blue-500/20 text-blue-400 border-blue-500/30',
        description: 'Feedback on physical interaction between digital elements'
    },
    'Progress indicators': {
        color: 'bg-amber-500/20 text-amber-400 border-amber-500/30',
        description: 'Gently inform about a starting, ending or advancing process'
    },
    'Incoming events': {
        color: 'bg-green-500/20 text-green-400 border-green-500/30',
        description: 'Grab attention toward a new event or status'
    },
};

export interface HapticResponse {
    success: boolean;
    waveform: string;
    deviceStatus: string;
    note: string;
}

export async function triggerWaveform(waveformApiName: string): Promise<HapticResponse | null> {
    try {
        const response = await fetch(`${API_BASE}/haptic/${waveformApiName}`, {
            method: 'POST',
        });
        if (!response.ok) {
            console.error('Failed to trigger waveform:', response.statusText);
            return null;
        }
        return await response.json();
    } catch (error) {
        console.error('Failed to trigger waveform:', error);
        return null;
    }
}

export async function healthCheck(): Promise<boolean> {
    try {
        const response = await fetch(API_BASE, { method: 'GET' });
        return response.ok;
    } catch {
        return false;
    }
}

// Convert waveform encoding to chart-friendly data
export function getWaveformPattern(waveformApiName: string): { time: number; value: number; type: string; hapticType: string }[] {
    const waveform = WAVEFORMS.find(w => w.api_name === waveformApiName);
    if (!waveform) return [];

    return waveform.waveform_encoding.map((item, index) => ({
        time: index,
        value: item.type === 'dot' ? 0.15 : (item.strength || 1) / 3,
        type: item.type,
        hapticType: item.haptic_type
    }));
}
