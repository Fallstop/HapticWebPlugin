<script lang="ts">
  import * as Slider from "$lib/components/ui/slider";
  import { triggerHapticWs } from "$lib/haptics.svelte";

  let value = $state(50);
  let lastTickValue = $state(50);
  const tickInterval = 5;

  function handleValueChange(newValue: number) {
    const currentTick = Math.floor(newValue / tickInterval) * tickInterval;
    const lastTick = Math.floor(lastTickValue / tickInterval) * tickInterval;

    if (currentTick !== lastTick) {
      triggerHapticWs("sharp_collision");
    }

    lastTickValue = newValue;
    value = newValue;
  }
</script>

<div
  class="flex flex-col items-center gap-4 p-6 rounded-xl bg-muted/30 border border-border">
  <h3 class="text-lg font-semibold">Haptic Slider</h3>
  <p class="text-sm text-muted-foreground text-center">
    Drag the slider and feel a tick every {tickInterval}%
  </p>

  <div class="w-full max-w-md space-y-4">
    <Slider.Root
      type="single"
      {value}
      onValueChange={handleValueChange}
      max={100}
      step={tickInterval}
      class="w-full &_[data-slot=slider-thumb]]:size-8 **:data-[slot=slider-thumb]:border-2" />

    <div class="flex justify-between text-xs text-muted-foreground">
      {#each [0, 25, 50, 75, 100] as tick}
        <span class="w-8 text-center">{tick}</span>
      {/each}
    </div>

    <div class="text-center">
      <span class="text-2xl font-bold text-primary">{value}%</span>
    </div>
  </div>
</div>
