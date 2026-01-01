<script lang="ts">
  import { Button } from "$lib/components/ui/button";
  import { triggerWaveform } from "$lib/haptics";
  import {
    getSelectedWaveform,
    getSelectedWaveformApiName,
  } from "$lib/haptics.svelte";
  import WaveformEncoding from "../WaveformEncoding.svelte";

  let isAnimating = $state(false);

  async function handleClick() {
    isAnimating = true;
    await triggerWaveform(getSelectedWaveformApiName());
    setTimeout(() => {
      isAnimating = false;
    }, 300);
  }
</script>

<div
  class="flex flex-col items-center gap-4 p-6 rounded-xl bg-linear-to-br from-primary/20 to-primary/5 border border-primary/20">
  <h3 class="text-lg font-semibold">Click to Feel</h3>
  <p class="text-sm text-muted-foreground text-center">
    Press the button to trigger <span class="font-semibold text-primary"
      >{getSelectedWaveform().name}</span>
  </p>
  <Button
    size="lg"
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
</div>
