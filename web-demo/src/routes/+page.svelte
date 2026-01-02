<script lang="ts">
  import ApiReference from "$lib/components/ApiReference.svelte";
  import AnimatedDemo from "$lib/components/demos/AnimatedDemo.svelte";
  import CTAButton from "$lib/components/demos/CTAButton.svelte";
  import HapticSlider from "$lib/components/demos/HapticSlider.svelte";
  import HoverZone from "$lib/components/demos/HoverZone.svelte";
  import StatusBanner from "$lib/components/StatusBanner.svelte";
  import { Badge } from "$lib/components/ui/badge";
  import { Separator } from "$lib/components/ui/separator";
  import * as Tabs from "$lib/components/ui/tabs";
  import WaveformSelector from "$lib/components/WaveformSelector.svelte";
  import { API_PORT } from "$lib/config";
  import MousePointer2 from "@lucide/svelte/icons/mouse-pointer-2";
  import Vibrate from "@lucide/svelte/icons/vibrate";
  import Zap from "@lucide/svelte/icons/zap";
</script>

<svelte:head>
  <title>Haptic Playground | MX Master 4</title>
</svelte:head>

<div class="min-h-screen bg-background">
  <!-- Hero Section -->
  <header class="relative overflow-hidden border-b border-border">
    <div
      class="absolute inset-0 bg-linear-to-br from-primary/10 via-transparent to-primary/5">
    </div>
    <div class="relative max-w-6xl mx-auto px-4 py-16 sm:py-24">
      <div class="flex flex-col items-center text-center gap-6">
        <div class="flex items-center gap-3">
          <div class="p-3 rounded-2xl bg-primary/20 border border-primary/30">
            <Vibrate class="w-8 h-8 text-primary" />
          </div>
          <Badge variant="secondary" class="text-xs">Logi Actions SDK</Badge>
        </div>

        <h1 class="text-4xl sm:text-5xl font-bold tracking-tight">
          Haptic Feedback
          <span class="text-primary"> Playground</span>
        </h1>

        <p class="text-lg text-muted-foreground max-w-2xl">
          Explore 16 unique tactile waveforms available through the MX Master 4
          haptic API. Select a waveform, then try the interactive demos to feel
          how haptic feedback can enhance your applications.
        </p>

        <div
          class="flex flex-wrap items-center justify-center gap-4 text-sm text-muted-foreground">
          <div class="flex items-center gap-2">
            <Zap class="w-4 h-4 text-primary" />
            <span>Local HTTPS API on port {API_PORT}</span>
          </div>
          <Separator orientation="vertical" class="h-4" />
          <div class="flex items-center gap-2">
            <MousePointer2 class="w-4 h-4 text-primary" />
            <span>Logitech MX Master 4</span>
          </div>
        </div>
      </div>
    </div>
  </header>

  <main class="max-w-6xl mx-auto px-4 py-8 space-y-12">
    <!-- Status Banners -->
    <StatusBanner />

    <!-- Waveform Selector Section -->
    <section class="space-y-4">
      <div class="flex items-center gap-3">
        <h2 class="text-2xl font-semibold">Choose a Waveform</h2>
        <Badge>16 available</Badge>
      </div>
      <p class="text-muted-foreground">
        Click any waveform to select it and immediately feel the haptic
        feedback. Your selection will be used in all the demos below.
      </p>
      <WaveformSelector />
    </section>

    <Separator />

    <!-- Interactive Demos Section -->
    <section class="space-y-6">
      <div>
        <h2 class="text-2xl font-semibold">Interactive Demos</h2>
        <p class="text-muted-foreground mt-1">
          Experience how haptic feedback can enhance different UI interactions
        </p>
      </div>

      <Tabs.Root value="buttons" class="w-full">
        <Tabs.List class="grid w-full grid-cols-4 max-w-md">
          <Tabs.Trigger value="buttons">Buttons</Tabs.Trigger>
          <Tabs.Trigger value="hover">Hover</Tabs.Trigger>
          <Tabs.Trigger value="slider">Slider</Tabs.Trigger>
          <Tabs.Trigger value="animation">Animation</Tabs.Trigger>
        </Tabs.List>

        <div class="mt-6">
          <Tabs.Content value="buttons">
            <CTAButton />
          </Tabs.Content>

          <Tabs.Content value="hover">
            <HoverZone />
          </Tabs.Content>

          <Tabs.Content value="slider">
            <HapticSlider />
          </Tabs.Content>

          <Tabs.Content value="animation">
            <AnimatedDemo />
          </Tabs.Content>
        </div>
      </Tabs.Root>
    </section>

    <Separator />

    <!-- Toys & Visualization Section -->
    <!-- <section class="space-y-6">
			<div>
				<h2 class="text-2xl font-semibold">Visualization & Toys</h2>
				<p class="text-muted-foreground mt-1">
					Explore waveform patterns and create your own haptic sequences
				</p>
			</div>
			
			<div class="grid md:grid-cols-2 gap-6">
				<WaveformGraph />
				<RhythmTapper />
			</div>
		</section> -->

    <!-- <Separator /> -->

    <!-- API Reference Section -->
    <section class="space-y-4">
      <h2 class="text-2xl font-semibold">API Reference</h2>
      <ApiReference />
    </section>
  </main>

  <!-- Footer -->
  <footer class="border-t border-border mt-16">
    <div class="max-w-6xl mx-auto px-4 py-8">
      <div
        class="flex flex-col sm:flex-row items-center justify-between gap-4 text-sm text-muted-foreground">
        <p>Haptic Web Plugin — Logi Actions SDK</p>
        <p>MX Master 4 Haptic Feedback API</p>
      </div>
    </div>
  </footer>
</div>
