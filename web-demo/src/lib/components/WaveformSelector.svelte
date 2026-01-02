<script lang="ts">
  import { Badge } from "$lib/components/ui/badge";
  import * as Card from "$lib/components/ui/card";
  import { CATEGORY_INFO, WAVEFORMS } from "$lib/haptics";
  import {
    getSelectedWaveform,
    getSelectedWaveformApiName,
    setSelectedWaveform,
    triggerHapticWs,
  } from "$lib/haptics.svelte";
  import Check from "@lucide/svelte/icons/check";
  import WaveformEncoding from "./WaveformEncoding.svelte";

  function handleSelect(waveformApiName: string) {
    setSelectedWaveform(waveformApiName);
    triggerHapticWs(waveformApiName);
  }

  // Group waveforms by category
  function getWaveformsByCategory() {
    const grouped: Record<string, typeof WAVEFORMS> = {};
    for (const waveform of WAVEFORMS) {
      if (!grouped[waveform.category]) {
        grouped[waveform.category] = [];
      }
      grouped[waveform.category].push(waveform);
    }
    return grouped;
  }
</script>

<!-- Legend -->
<div class="mt-6 p-3 rounded-lg bg-muted/30 border border-border">
  <p class="text-xs text-muted-foreground mb-2 font-medium">Waveform Legend</p>
  <div class="flex flex-wrap gap-4 text-xs">
    <div class="flex items-center gap-2">
      <div class="w-3 h-4 rounded-sm bg-blue-400"></div>
      <span class="text-muted-foreground">Knock</span>
    </div>
    <div class="flex items-center gap-2">
      <div class="w-3 h-4 rounded-sm bg-green-400"></div>
      <span class="text-muted-foreground">Vibration</span>
    </div>
    <div class="flex items-center gap-2">
      <div class="w-2 h-2 rounded-full bg-rose-400"></div>
      <span class="text-muted-foreground">Silent gap</span>
    </div>
    <div class="flex items-center gap-2">
      <div class="flex items-end gap-0.5">
        <div class="w-1.5 h-2 bg-muted-foreground/50 rounded-sm"></div>
        <div class="w-1.5 h-3 bg-muted-foreground/50 rounded-sm"></div>
        <div class="w-1.5 h-4 bg-muted-foreground/50 rounded-sm"></div>
      </div>
      <span class="text-muted-foreground">Strength 1-2-3</span>
    </div>
  </div>
</div>

<!-- Selected Waveform Display -->
<div class="mb-6 p-4 rounded-xl bg-primary/10 border-2 border-primary/30">
  <div class="flex flex-col sm:flex-row items-center gap-4">
    <div class="flex-1 text-center sm:text-left">
      <p class="text-xs text-muted-foreground uppercase tracking-wide mb-1">
        Currently Selected
      </p>
      <h3 class="text-xl font-bold text-primary flex items-center gap-2">
        {getSelectedWaveform().name}
        <Badge
          variant="outline"
          class="mt-1 {CATEGORY_INFO[getSelectedWaveform().category]?.color}">
          {getSelectedWaveform().category}
        </Badge>
      </h3>
    </div>
    <div class="p-4 rounded-lg bg-background/50">
      <WaveformEncoding
        encoding={getSelectedWaveform().waveform_encoding}
        size="lg" />
    </div>
  </div>
</div>

<!-- Waveform Grid by Category -->
{#each Object.entries(getWaveformsByCategory()) as [category, waveforms]}
  <div class="mb-6">
    <div class="flex items-center gap-3 mb-3">
      <h4 class="text-sm font-semibold text-muted-foreground">{category}</h4>
      <span class="text-xs text-muted-foreground/60">
        {CATEGORY_INFO[category]?.description}
      </span>
    </div>
    <div
      class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3">
      {#each waveforms as waveform}
        {@const isSelected = getSelectedWaveformApiName() === waveform.api_name}
        <button
          class="text-left focus:outline-none focus-visible:ring-2 focus-visible:ring-primary rounded-lg"
          onclick={() => handleSelect(waveform.api_name)}>
          <Card.Root
            class="relative h-full transition-all duration-200 hover:scale-[1.02] cursor-pointer {isSelected
              ? 'ring-2 ring-primary bg-primary/10'
              : 'hover:bg-muted/50'}">
            {#if isSelected}
              <div
                class="absolute top-2 right-2 w-5 h-5 rounded-full bg-primary flex items-center justify-center">
                <Check class="w-3 h-3 text-primary-foreground" />
              </div>
            {/if}
            <Card.Content class="p-3 flex flex-col items-center gap-2">
              <div class="py-2">
                <WaveformEncoding
                  encoding={waveform.waveform_encoding}
                  size="sm" />
              </div>
              <span
                class="text-xs font-medium text-center leading-tight bg-background/50 px-2 py-1 rounded select-text cursor-text font-mono"
                role="button"
                tabindex="0"
                onclick={(e) => e.stopPropagation()}
                onkeydown={(e) => {
                  if (e.key === "Enter" || e.key === " ") e.stopPropagation();
                }}>
                {waveform.api_name}
              </span>
            </Card.Content>
          </Card.Root>
        </button>
      {/each}
    </div>
  </div>
{/each}
