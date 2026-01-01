import { WAVEFORMS, type Waveform } from './haptics';

// Svelte 5 reactive state for selected waveform
let selectedWaveformApiName = $state<string>('sharp_collision');

export function getSelectedWaveform(): Waveform {
    return WAVEFORMS.find(w => w.api_name === selectedWaveformApiName) || WAVEFORMS[0];
}

export function getSelectedWaveformApiName(): string {
    return selectedWaveformApiName;
}

export function setSelectedWaveform(apiName: string): void {
    selectedWaveformApiName = apiName;
}