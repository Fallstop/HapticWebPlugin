<script lang="ts">
  import { isConnected, triggerHaptic } from "$lib/connection.svelte";
  import MousePointer from "@lucide/svelte/icons/mouse-pointer";
  import Sparkles from "@lucide/svelte/icons/sparkles";

  const WAVEFORM_ID = "damp_state_change";

  let isHovered = $state(false);
  const connected = $derived(isConnected());

  function handleMouseEnter() {
    isHovered = true;
    if (connected) triggerHaptic(WAVEFORM_ID);
  }

  function handleMouseLeave() {
    isHovered = false;
    if (connected) triggerHaptic(WAVEFORM_ID);
  }
</script>

<div
  class="flex flex-col items-center gap-4 p-6 rounded-xl bg-muted/30 border border-border">
  <h3 class="text-lg font-semibold">Hover Zone</h3>
  <p class="text-sm text-muted-foreground text-center">
    Move your mouse in and out of the zone below
  </p>

  <div
    role="button"
    tabindex="0"
    class="w-48 h-32 rounded-lg border-2 border-dashed flex items-center justify-center transition-all duration-300
			{!connected ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'}
			{isHovered && connected
      ? 'border-primary bg-primary/20 scale-105'
      : 'border-muted-foreground/30 bg-muted/50 hover:border-muted-foreground/50'}"
    onmouseenter={handleMouseEnter}
    onmouseleave={handleMouseLeave}>
    <span
      class="transition-transform duration-300 {isHovered && connected
        ? 'scale-125 text-primary'
        : 'text-muted-foreground'}">
      {#if isHovered && connected}
        <Sparkles class="w-8 h-8" />
      {:else}
        <MousePointer class="w-8 h-8" />
      {/if}
    </span>
  </div>

  <p class="text-xs text-muted-foreground">
    {#if !connected}
      Connect your mouse to enable
    {:else if isHovered}
      Inside zone!
    {:else}
      Hover over the zone
    {/if}
  </p>
</div>
