<script lang="ts">
  import AnimatedDemo from "$lib/components/demos/AnimatedDemo.svelte";
  import CTAButton from "$lib/components/demos/CTAButton.svelte";
  import FormValidation from "$lib/components/demos/FormValidation.svelte";
  import HapticSlider from "$lib/components/demos/HapticSlider.svelte";
  import HoverZone from "$lib/components/demos/HoverZone.svelte";
  import { Button } from "$lib/components/ui/button";
  import { Separator } from "$lib/components/ui/separator";
  import * as Tabs from "$lib/components/ui/tabs";
  import WaveformSelector from "$lib/components/WaveformSelector.svelte";
  import {
    connect,
    hasInitiatedConnection,
    isConnected,
  } from "$lib/connection.svelte";
  import Ear from "@lucide/svelte/icons/ear";
  import Vibrate from "@lucide/svelte/icons/vibrate";

  const connected = $derived(isConnected());
  const hasInitiated = $derived(hasInitiatedConnection());

  function handleConnect() {
    connect();
  }
</script>

<svelte:head>
  <title>Playground — HapticWeb</title>
  <meta
    name="description"
    content="Try all 16 haptic waveforms with interactive demos." />
</svelte:head>

<div class="max-w-6xl mx-auto px-4 py-12 space-y-12">
  <!-- Header -->
  <div class="space-y-4">
    <h1 class="text-4xl font-bold">Haptic Playground</h1>
    <p class="text-lg text-muted-foreground max-w-2xl">
      Explore all available waveforms and try interactive demos. Select a
      waveform to feel it instantly.
    </p>
  </div>

  <!-- Connection Banner -->
  {#if !connected}
    <div
      class="rounded-xl border border-border bg-linear-to-br from-primary/5 via-transparent to-primary/10 p-6">
      <div class="flex flex-col sm:flex-row items-start sm:items-center gap-4">
        <div class="flex-1 space-y-1">
          <h3 class="font-semibold">Connect your MX Master 4</h3>
          <p class="text-sm text-muted-foreground">
            {#if hasInitiated && !connected}
              Couldn't connect. Make sure the plugin is installed and Logi
              Options+ is running.
            {:else}
              Connect to start feeling the haptic feedback.
            {/if}
          </p>
        </div>
        <Button onclick={handleConnect}>
          <Vibrate class="w-4 h-4 mr-2" />
          {hasInitiated ? "Try Again" : "Connect Mouse"}
        </Button>
      </div>
    </div>
  {:else}
    <div class="rounded-xl border border-green-500/30 bg-green-500/10 p-4">
      <div class="flex items-center gap-3">
        <div class="w-2 h-2 rounded-full bg-green-400 animate-pulse"></div>
        <span class="text-green-300 font-medium">Connected</span>
        <span class="text-green-200/70 text-sm"
          >The Haptic Web plugin is reachable and working.</span>
      </div>
    </div>
  {/if}

  <!-- Waveform Selector -->
  <section class="space-y-4">
    <div class="flex items-center gap-3">
      <h2 class="text-2xl font-semibold">Choose a Waveform</h2>
    </div>
    <p class="text-muted-foreground">
      Click any waveform to select it and immediately feel the haptic feedback.
      The Button and Hover demos use your selection.
    </p>
    <WaveformSelector />
  </section>

  <Separator />

  <!-- Interactive Demos -->
  <section class="space-y-6">
    <div>
      <h2 class="text-2xl font-semibold">Interactive Demos</h2>
      <p class="text-muted-foreground mt-1">
        Experience how haptic feedback can enhance different UI interactions
      </p>
    </div>

    <Tabs.Root value="buttons" class="w-full">
      <Tabs.List class="grid w-full grid-cols-5 max-w-lg">
        <Tabs.Trigger value="buttons">Buttons</Tabs.Trigger>
        <Tabs.Trigger value="form">Form</Tabs.Trigger>
        <Tabs.Trigger value="hover">Hover</Tabs.Trigger>
        <Tabs.Trigger value="slider">Slider</Tabs.Trigger>
        <Tabs.Trigger value="animation">Animation</Tabs.Trigger>
      </Tabs.List>

      <div class="mt-6">
        <Tabs.Content value="buttons">
          <CTAButton />
        </Tabs.Content>

        <Tabs.Content value="form">
          <FormValidation />
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

  <!-- Tips -->
  <section class="space-y-4">
    <h2 class="text-2xl font-semibold">Good to Know</h2>
    <div class="grid sm:grid-cols-2 gap-4">
      <div class="p-4 rounded-lg border border-border bg-card">
        <div class="flex items-center gap-2 mb-2">
          <Vibrate class="w-4 h-4 text-primary" />
          <h3 class="font-medium">Haptic strength</h3>
        </div>
        <p class="text-sm text-muted-foreground">
          The intensity can only be adjusted in Logi Options+ under Haptic
          Feedback &rightarrow; Strength. There's no API control for this.
        </p>
      </div>
      <div class="p-4 rounded-lg border border-border bg-card">
        <div class="flex items-center gap-2 mb-2">
          <Ear class="w-4 h-4 text-primary" />
          <h3 class="font-medium">Audible feedback</h3>
        </div>
        <p class="text-sm text-muted-foreground">
          The haptic motor is often audible, even when you're not holding the
          mouse. This makes it useful for potentially replacing audio
          notifications.
        </p>
      </div>
    </div>
  </section>
</div>
