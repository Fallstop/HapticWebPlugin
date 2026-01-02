<script lang="ts">
  import { Button } from "$lib/components/ui/button";
  import {
    getSelectedWaveform,
    isConnected,
    triggerSelectedHaptic,
  } from "$lib/connection.svelte";
  import WaveformEncoding from "../WaveformEncoding.svelte";

  let isAnimating = $state(false);
  const connected = $derived(isConnected());

  function handleClick() {
    if (!connected) return;
    isAnimating = true;
    triggerSelectedHaptic();
    setTimeout(() => {
      isAnimating = false;
    }, 300);
  }
</script>

<div
  class="flex flex-col items-center gap-4 p-6 rounded-xl bg-muted/30 border border-border">
  <h3 class="text-lg font-semibold">Click to Feel</h3>
  <p class="text-sm text-muted-foreground text-center">
    Press the button to trigger <span class="font-semibold text-primary"
      >{getSelectedWaveform().name}</span>
  </p>
  <Button
    size="lg"
    disabled={!connected}
    class="text-lg px-4 py-6 transition-transform {isAnimating
      ? 'scale-95'
      : ''}"
    onclick={handleClick}>
    <span class="mr-2">Trigger Haptic</span>
    <div class="bg-background px-2 py-1 rounded-lg">
      <WaveformEncoding
        encoding={getSelectedWaveform().waveform_encoding}
        size="sm" />
    </div>
  </Button>
  {#if !connected}
    <p class="text-xs text-muted-foreground">Connect your mouse to enable</p>
  {/if}
</div>
