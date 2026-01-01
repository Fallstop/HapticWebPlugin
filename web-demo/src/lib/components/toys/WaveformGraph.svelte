<script lang="ts">
  import { Button } from "$lib/components/ui/button";
  import { triggerWaveform } from "$lib/haptics";
  import {
    getSelectedWaveform,
    getSelectedWaveformApiName,
  } from "$lib/haptics.svelte";
  import Loader from "@lucide/svelte/icons/loader";
  import Play from "@lucide/svelte/icons/play";

  let isPlaying = $state(false);
  let currentStep = $state(-1);

  function playWaveform() {
    if (isPlaying) return;
    isPlaying = true;
    currentStep = -1;

    triggerWaveform(getSelectedWaveformApiName());

    const encoding = getSelectedWaveform().waveform_encoding;
    const stepDuration = 100; // ms per step
    let step = 0;

    const interval = setInterval(() => {
      currentStep = step;
      step++;
      if (step >= encoding.length) {
        clearInterval(interval);
        setTimeout(() => {
          isPlaying = false;
          currentStep = -1;
        }, stepDuration);
      }
    }, stepDuration);
  }
</script>

<div
  class="flex flex-col gap-4 p-6 rounded-xl bg-muted/30 border border-border">
  <div class="flex items-center justify-between">
    <div>
      <h3 class="text-lg font-semibold">Waveform Visualizer</h3>
      <p class="text-sm text-muted-foreground">
        {getSelectedWaveform().name}
      </p>
    </div>
    <Button onclick={playWaveform} disabled={isPlaying} size="sm">
      {#if isPlaying}
        <Loader class="w-4 h-4 mr-1 animate-spin" /> Playing...
      {:else}
        <Play class="w-4 h-4 mr-1" /> Play
      {/if}
    </Button>
  </div>

  <!-- Large animated waveform display -->
  <div
    class="flex items-center justify-center gap-1 h-24 p-4 bg-background/50 rounded-lg">
    {#each getSelectedWaveform().waveform_encoding as item, i}
      {@const isActive = currentStep === i}
      {@const height =
        item.type === "dot" ? 8 : ((item.strength || 1) / 3) * 80}
      {@const color =
        item.haptic_type === "knock"
          ? "bg-blue-400"
          : item.haptic_type === "vibration"
            ? "bg-green-400"
            : "bg-rose-400"}

      {#if item.type === "bar"}
        <div
          class="w-4 rounded-sm transition-all duration-100 {color} {isActive
            ? 'ring-2 ring-white ring-offset-2 ring-offset-background scale-110'
            : ''}"
          style="height: {height}px; opacity: {isActive ? 1 : 0.6};">
        </div>
      {:else}
        <div
          class="w-3 h-3 rounded-full self-center transition-all duration-100 {color} {isActive
            ? 'ring-2 ring-white ring-offset-2 ring-offset-background scale-125'
            : ''}"
          style="opacity: {isActive ? 1 : 0.6};">
        </div>
      {/if}
    {/each}
  </div>

  <!-- Encoding breakdown -->
  <div class="text-xs text-muted-foreground">
    <p class="mb-2">Pattern breakdown:</p>
    <div class="flex flex-wrap gap-1">
      {#each getSelectedWaveform().waveform_encoding as item, i}
        <span
          class="px-2 py-1 rounded bg-muted {currentStep === i
            ? 'ring-1 ring-primary'
            : ''}">
          {i + 1}: {item.type === "bar"
            ? `${item.haptic_type} (${item.strength})`
            : "gap"}
        </span>
      {/each}
    </div>
  </div>
</div>
