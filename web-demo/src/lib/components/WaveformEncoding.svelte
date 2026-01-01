<script lang="ts">
  import type { WaveformEncodingItem } from "$lib/haptics";

  interface Props {
    encoding: WaveformEncodingItem[];
    size?: "sm" | "md" | "lg";
  }

  let { encoding, size = "md" }: Props = $props();

  const sizeConfig = {
    sm: { barWidth: 4, dotSize: 4, gap: 1, maxHeight: 24 },
    md: { barWidth: 6, dotSize: 5, gap: 2, maxHeight: 36 },
    lg: { barWidth: 8, dotSize: 6, gap: 3, maxHeight: 48 },
  };

  function getBarHeight(
    strength: number | undefined,
    maxHeight: number
  ): number {
    const s = strength || 1;
    return (s / 3) * maxHeight;
  }

  function getColor(hapticType: string): string {
    switch (hapticType) {
      case "knock":
        return "bg-blue-400";
      case "vibration":
        return "bg-green-400";
      case "silent":
        return "bg-rose-400";
      default:
        return "bg-muted-foreground";
    }
  }
</script>

<div
  class="flex justify-center items-center"
  style="gap: {sizeConfig[size].gap}px; height: {sizeConfig[size]
    .maxHeight}px;">
  {#each encoding as item}
    {#if item.type === "bar"}
      <div
        class="rounded-sm {getColor(item.haptic_type)}"
        style="width: {sizeConfig[size].barWidth}px; height: {getBarHeight(
          item.strength,
          sizeConfig[size].maxHeight
        )}px;">
      </div>
    {:else}
      <div
        class="rounded-full {getColor(item.haptic_type)} self-center"
        style="width: {sizeConfig[size].dotSize}px; height: {sizeConfig[size]
          .dotSize}px;">
      </div>
    {/if}
  {/each}
</div>
