<script lang="ts">
  import { Badge } from "$lib/components/ui/badge";
  import { Button } from "$lib/components/ui/button";
  import { Separator } from "$lib/components/ui/separator";
  import {
    connect,
    getConnectionState,
    hasInitiatedConnection,
    isConnected,
  } from "$lib/connection.svelte";
  import ArrowRight from "@lucide/svelte/icons/arrow-right";
  import Check from "@lucide/svelte/icons/check";
  import CheckCircle from "@lucide/svelte/icons/check-circle";
  import Code from "@lucide/svelte/icons/code";
  import Download from "@lucide/svelte/icons/download";
  import Vibrate from "@lucide/svelte/icons/vibrate";
  import Zap from "@lucide/svelte/icons/zap";

  const connected = $derived(isConnected());
  const hasInitiated = $derived(hasInitiatedConnection());
  const connectionState = $derived(getConnectionState());

  function handleConnect() {
    connect();
  }
</script>

<svelte:head>
  <title>HapticWeb — MX Master 4 Haptic Feedback API</title>
  <meta
    name="description"
    content="A Logi Actions SDK plugin that exposes MX Master 4 haptic feedback via a local HTTPS server and WebSocket API." />
</svelte:head>

<!-- Hero Section -->
<section class="relative overflow-hidden border-b border-border">
  <div
    class="absolute inset-0 bg-linear-to-br from-primary/10 via-transparent to-primary/5">
  </div>

  <div class="relative max-w-6xl mx-auto px-4 py-16 sm:py-24">
    <div class="grid lg:grid-cols-2 gap-12 items-center">
      <!-- Left: Text content -->
      <div class="space-y-6">
        <div class="flex items-center gap-3">
          <Badge variant="secondary">Logi Actions SDK</Badge>
          <Badge variant="outline">Open Source</Badge>
        </div>

        <h1 class="text-4xl sm:text-5xl font-bold tracking-tight">
          Haptic Feedback for the
          <span class="text-primary"> Web</span>
        </h1>

        <p class="text-lg text-muted-foreground">
          A plugin that exposes MX Master 4 haptic feedback through a local
          HTTPS server and WebSocket API. Add tactile feedback to your web apps,
          games, and creative tools.
        </p>

        <div class="flex flex-wrap gap-3">
          <Button href="/install" size="lg">
            <Download class="w-4 h-4 mr-2" />
            Install Plugin
          </Button>
          <Button href="/playground" variant="outline" size="lg">
            <Zap class="w-4 h-4 mr-2" />
            Try Playground
          </Button>
        </div>

        <div
          class="flex flex-wrap items-center gap-4 text-sm text-muted-foreground pt-2">
          <div class="flex items-center gap-2">
            <Check class="w-4 h-4 text-green-500" />
            <span>16 haptic waveforms</span>
          </div>
          <div class="flex items-center gap-2">
            <Check class="w-4 h-4 text-green-500" />
            <span>REST + WebSocket API</span>
          </div>
          <div class="flex items-center gap-2">
            <Check class="w-4 h-4 text-green-500" />
            <span>Works in any browser</span>
          </div>
        </div>
      </div>

      <!-- Right: Mouse image -->
      <div class="relative flex justify-center lg:justify-end">
        <div class="relative">
          <img
            src="/mx-master4.webp"
            alt="Logitech MX Master 4 Mouse"
            class="relative z-0 w-full max-w-md drop-shadow-2xl" />
          <div
            class="absolute z-20 -inset-4 bg-linear-to-r from-primary/20 via-primary/10 to-transparent rounded-3xl blur-2xl">
          </div>
        </div>
      </div>
    </div>
  </div>
</section>

<!-- Quick Start Section -->
<section class="max-w-6xl mx-auto px-4 py-16">
  <div class="text-center space-y-4 mb-12">
    <h2 class="text-3xl font-bold">Get Started in Minutes</h2>
    <p class="text-muted-foreground max-w-2xl mx-auto">
      Install the plugin, connect your MX Master 4, and start adding haptic
      feedback to your projects.
    </p>
  </div>

  <div class="grid md:grid-cols-3 gap-8">
    <div class="space-y-4 p-6 rounded-xl border border-border bg-card">
      <div
        class="w-10 h-10 rounded-lg bg-primary/20 flex items-center justify-center">
        <Download class="w-5 h-5 text-primary" />
      </div>
      <h3 class="text-xl font-semibold">1. Install</h3>
      <p class="text-muted-foreground text-sm">
        Download the plugin and install it through Logi Options+. Takes less
        than a minute.
      </p>
      <a
        href="/install"
        class="inline-flex items-center text-sm text-primary hover:underline">
        Installation guide <ArrowRight class="w-4 h-4 ml-1" />
      </a>
    </div>

    <div class="space-y-4 p-6 rounded-xl border border-border bg-card">
      <div
        class="w-10 h-10 rounded-lg bg-primary/20 flex items-center justify-center">
        <Code class="w-5 h-5 text-primary" />
      </div>
      <h3 class="text-xl font-semibold">2. Integrate</h3>
      <p class="text-muted-foreground text-sm">
        Add a few lines of JavaScript to trigger haptic feedback in your app via
        WebSocket or REST.
      </p>
      <a
        href="/integrate"
        class="inline-flex items-center text-sm text-primary hover:underline">
        Integration guide <ArrowRight class="w-4 h-4 ml-1" />
      </a>
    </div>

    <div class="space-y-4 p-6 rounded-xl border border-border bg-card">
      <div
        class="w-10 h-10 rounded-lg bg-primary/20 flex items-center justify-center">
        <Vibrate class="w-5 h-5 text-primary" />
      </div>
      <h3 class="text-xl font-semibold">3. Feel</h3>
      <p class="text-muted-foreground text-sm">
        Choose from 16 unique waveforms for clicks, drags, alerts, and more.
      </p>
      <a
        href="/playground"
        class="inline-flex items-center text-sm text-primary hover:underline">
        Try the playground <ArrowRight class="w-4 h-4 ml-1" />
      </a>
    </div>
  </div>
</section>

<Separator class="max-w-6xl mx-auto" />

<!-- Try it now section -->
<section class="max-w-6xl mx-auto px-4 py-16">
  <div
    class="rounded-2xl border border-border bg-linear-to-br from-primary/5 via-transparent to-primary/10 p-8 sm:p-12">
    <div class="flex flex-col lg:flex-row items-center gap-8">
      <div class="flex-1 space-y-4">
        <h2 class="text-2xl sm:text-3xl font-bold">
          Got the plugin installed?
        </h2>
        <p class="text-muted-foreground">
          Connect your MX Master 4 to test it right here. This will request
          permission to access your local haptics service.
        </p>
      </div>

      <div class="flex flex-col items-center gap-3">
        {#if connected}
          <div class="flex items-center gap-2 text-green-400 font-medium">
            <CheckCircle class="w-5 h-5" />
            <span>Connected!</span>
          </div>
          <Button href="/playground" variant="default" size="lg">
            Open Playground <ArrowRight class="w-4 h-4 ml-2" />
          </Button>
        {:else if hasInitiated && !connected}
          <p class="text-sm text-muted-foreground text-center">
            Couldn't connect. Make sure the plugin is installed and Logi
            Options+ is running.
          </p>
          <Button onclick={handleConnect} variant="outline" size="lg">
            Try Again
          </Button>
        {:else}
          <Button onclick={handleConnect} size="lg">
            <Vibrate class="w-4 h-4 mr-2" />
            Connect Mouse
          </Button>
        {/if}
      </div>
    </div>
  </div>
</section>

<!-- Requirements callout -->
<section class="max-w-6xl mx-auto px-4 pb-16">
  <div class="rounded-xl border border-amber-500/30 bg-amber-500/10 p-6">
    <div class="flex gap-4">
      <img
        src="/mx-master4.webp"
        alt="MX Master 4"
        class="w-24 h-auto object-contain hidden sm:block" />
      <div class="space-y-2">
        <h3 class="font-semibold text-amber-200">Hardware Requirement</h3>
        <p class="text-sm text-amber-100/80">
          This plugin requires a <strong>Logitech MX Master 4</strong> mouse
          with <strong>Logi Options+</strong> installed. The MX Master 4 is the only
          Logitech mouse with a haptic motor that can be controlled via the Actions
          SDK.
        </p>
      </div>
    </div>
  </div>
</section>
