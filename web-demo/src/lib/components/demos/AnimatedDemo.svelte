<script lang="ts">
  import { Button } from "$lib/components/ui/button";
  import { triggerHapticWs } from "$lib/haptics.svelte";
  import Play from "@lucide/svelte/icons/play";
  import Square from "@lucide/svelte/icons/square";

  let isPlaying = $state(false);
  let ballPosition = $state(0);
  let animationFrame: number;
  let bounceCount = $state(0);

  const WAVEFORM_ID = "subtle_collision";

  function startAnimation() {
    if (isPlaying) return;
    isPlaying = true;
    bounceCount = 0;
    animate();
  }

  function stopAnimation() {
    isPlaying = false;
    if (animationFrame) {
      cancelAnimationFrame(animationFrame);
    }
    ballPosition = 0;
  }

  function animate() {
    if (!isPlaying || bounceCount >= 5) {
      stopAnimation();
      return;
    }

    const duration = 600;
    const startTime = performance.now();

    function frame(currentTime: number) {
      if (!isPlaying) return;

      const elapsed = currentTime - startTime;
      const progress = Math.min(elapsed / duration, 1);

      // Parabolic bounce motion
      const height = 1 - Math.pow(2 * progress - 1, 2);
      ballPosition = height * 100;

      if (progress >= 1) {
        // Ball hit the ground - trigger haptic
        triggerHapticWs(WAVEFORM_ID);
        bounceCount++;

        if (bounceCount < 5) {
          animate();
        } else {
          stopAnimation();
        }
      } else {
        animationFrame = requestAnimationFrame(frame);
      }
    }

    animationFrame = requestAnimationFrame(frame);
  }
</script>

<div
  class="flex flex-col items-center gap-4 p-6 rounded-xl bg-muted/30 border border-border">
  <h3 class="text-lg font-semibold">Bouncing Ball</h3>
  <p class="text-sm text-muted-foreground text-center">
    Watch the ball bounce and feel each impact
  </p>

  <div
    class="relative w-full h-32 bg-linear-to-b from-transparent to-muted/50 rounded-lg overflow-hidden">
    <!-- Ground line -->
    <div class="absolute bottom-0 left-0 right-0 h-1 bg-primary/50"></div>

    <!-- Ball -->
    <div
      class="absolute left-1/2 -translate-x-1/2 w-8 h-8 rounded-full bg-primary shadow-lg shadow-primary/50 transition-none"
      style="bottom: calc({ballPosition}% + 4px)">
      <div class="absolute inset-1 rounded-full bg-primary-foreground/20"></div>
    </div>
  </div>

  <div class="flex gap-2">
    <Button onclick={startAnimation} disabled={isPlaying}>
      <Play class="w-4 h-4 mr-1" /> Start
    </Button>
    <Button variant="outline" onclick={stopAnimation} disabled={!isPlaying}>
      <Square class="w-4 h-4 mr-1" /> Stop
    </Button>
  </div>

  {#if isPlaying}
    <p class="text-xs text-muted-foreground">
      Bounce {bounceCount}/5
    </p>
  {/if}
</div>
