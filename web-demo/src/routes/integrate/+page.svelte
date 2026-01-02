<script lang="ts">
  import { Button } from "$lib/components/ui/button";
  import { Separator } from "$lib/components/ui/separator";
  import { connect, isConnected, triggerHaptic } from "$lib/connection.svelte";
  import { WAVEFORMS } from "$lib/haptics";
  import Check from "@lucide/svelte/icons/check";
  import Copy from "@lucide/svelte/icons/copy";
  import ExternalLink from "@lucide/svelte/icons/external-link";
  import Vibrate from "@lucide/svelte/icons/vibrate";

  const connected = $derived(isConnected());

  let copiedWs = $state(false);
  let copiedRest = $state(false);

  function handleConnect() {
    connect();
  }

  function copyCode(code: string, which: "ws" | "rest") {
    navigator.clipboard.writeText(code);
    if (which === "ws") {
      copiedWs = true;
      setTimeout(() => (copiedWs = false), 2000);
    } else {
      copiedRest = true;
      setTimeout(() => (copiedRest = false), 2000);
    }
  }

  const wsExample = `const ws = new WebSocket('wss://local.jmw.nz:41443/ws');

ws.onopen = () => {
  console.log('Connected to HapticWeb');
  
  // Trigger "sharp_collision" (index 0)
  ws.send(new Uint8Array([0]));
  
  // Trigger "completed" (index 7)
  ws.send(new Uint8Array([7]));
};`;

  const restExample = `// Trigger a haptic waveform
await fetch('https://local.jmw.nz:41443/haptic/sharp_collision', {
  method: 'POST',
  body: ''
});

// Get available waveforms
const waveforms = await fetch('https://local.jmw.nz:41443/waveforms')
  .then(res => res.json());`;

  function testHaptic(name: string) {
    triggerHaptic(name);
  }
</script>

<svelte:head>
  <title>Integrate — HapticWeb</title>
  <meta
    name="description"
    content="Add haptic feedback to your web apps with HapticWeb." />
</svelte:head>

<div class="max-w-6xl mx-auto px-4 py-12">
  <!-- Header -->
  <div class="space-y-4 mb-12">
    <h1 class="text-4xl font-bold">Add Haptics to Your App</h1>
    <p class="text-lg text-muted-foreground">
      Two ways to trigger haptic feedback: WebSocket for low-latency, or REST
      for simplicity.
    </p>
  </div>

  <!-- SDK Docs callout -->
  <div class="rounded-xl border border-border bg-muted/30 p-6 mb-12">
    <h3 class="font-semibold mb-2">Building a native plugin?</h3>
    <p class="text-sm text-muted-foreground mb-3">
      If you're implementing haptics directly using the Logitech Actions SDK
      (C#), check out the official documentation for best practices.
    </p>
    <a
      href="https://logitech.github.io/actions-sdk-docs/csharp/Haptics-Overview/"
      target="_blank"
      rel="noopener noreferrer"
      class="inline-flex items-center gap-1 text-sm text-primary hover:underline">
      Logitech Actions SDK Haptics Docs <ExternalLink class="w-3 h-3" />
    </a>
  </div>

  <!-- Connection check -->
  {#if !connected}
    <div
      class="rounded-xl border border-amber-500/30 bg-amber-500/10 p-6 mb-12">
      <div class="flex flex-col sm:flex-row items-start sm:items-center gap-4">
        <div class="flex-1">
          <h3 class="font-semibold text-amber-200">
            Connect to test the examples
          </h3>
          <p class="text-sm text-amber-100/80 mt-1">
            Connect your mouse to try the live examples on this page.
          </p>
        </div>
        <Button onclick={handleConnect} size="sm">
          <Vibrate class="w-4 h-4 mr-2" />
          Connect Mouse
        </Button>
      </div>
    </div>
  {/if}

  <!-- WebSocket Section -->
  <section class="mb-12">
    <div class="mb-6">
      <h2 class="text-2xl font-semibold">WebSocket API</h2>
      <p class="text-sm text-muted-foreground">
        Recommended for real-time, low-latency feedback
      </p>
    </div>

    <div class="space-y-4">
      <div class="rounded-lg border border-border overflow-hidden">
        <div
          class="flex items-center justify-between px-4 py-2 bg-muted border-b border-border">
          <span class="text-sm font-medium">JavaScript</span>
          <button
            onclick={() => copyCode(wsExample, "ws")}
            class="p-1.5 rounded hover:bg-background transition-colors">
            {#if copiedWs}
              <Check class="w-4 h-4 text-green-400" />
            {:else}
              <Copy class="w-4 h-4 text-muted-foreground" />
            {/if}
          </button>
        </div>
        <pre class="p-4 text-sm overflow-x-auto"><code>{wsExample}</code></pre>
      </div>

      <div class="p-4 rounded-lg bg-muted/30 border border-border">
        <h4 class="font-medium mb-2">How it works</h4>
        <ul
          class="text-sm text-muted-foreground space-y-1 list-disc list-inside">
          <li>
            Connect to <code
              class="px-1 py-0.5 rounded bg-muted text-foreground"
              >wss://local.jmw.nz:41443/ws</code>
          </li>
          <li>Send a single byte containing the waveform index (0-15)</li>
          <li>
            No response is sent back — connection stays open for repeated
            triggers
          </li>
          <li>
            The connection uses a pre-allocated buffer for zero-allocation sends
          </li>
        </ul>
      </div>
    </div>
  </section>

  <Separator class="mb-12" />

  <!-- REST Section -->
  <section class="mb-12">
    <div class="mb-6">
      <h2 class="text-2xl font-semibold">REST API</h2>
      <p class="text-sm text-muted-foreground">
        Simple HTTP requests, works from any language
      </p>
    </div>

    <div class="space-y-4">
      <div class="rounded-lg border border-border overflow-hidden">
        <div
          class="flex items-center justify-between px-4 py-2 bg-muted border-b border-border">
          <span class="text-sm font-medium">JavaScript</span>
          <button
            onclick={() => copyCode(restExample, "rest")}
            class="p-1.5 rounded hover:bg-background transition-colors">
            {#if copiedRest}
              <Check class="w-4 h-4 text-green-400" />
            {:else}
              <Copy class="w-4 h-4 text-muted-foreground" />
            {/if}
          </button>
        </div>
        <pre class="p-4 text-sm overflow-x-auto"><code
            >{restExample}</code></pre>
      </div>

      <div class="p-4 rounded-lg bg-muted/30 border border-border">
        <h4 class="font-medium mb-2">Note about POST requests</h4>
        <p class="text-sm text-muted-foreground">
          POST requests require a <code
            class="px-1 py-0.5 rounded bg-muted text-foreground"
            >Content-Length</code>
          header. When using curl, include
          <code class="px-1 py-0.5 rounded bg-muted text-foreground"
            >-d ''</code> to send an empty body with the proper header, otherwise
          the request will hang.
        </p>
      </div>
    </div>
  </section>

  <Separator class="mb-12" />

  <!-- Waveform Quick Reference -->
  <section class="mb-12">
    <h2 class="text-2xl font-semibold mb-6">Waveform Quick Reference</h2>
    <p class="text-muted-foreground mb-4">
      Click any waveform to trigger it{connected
        ? ""
        : " (connect your mouse first)"}.
    </p>

    <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-2">
      {#each WAVEFORMS as waveform, index}
        <button
          onclick={() => testHaptic(waveform.api_name)}
          disabled={!connected}
          class="p-3 rounded-lg border border-border bg-card hover:bg-accent disabled:opacity-50 disabled:cursor-not-allowed text-left transition-colors">
          <div class="text-xs text-muted-foreground">Index {index}</div>
          <div class="font-mono text-sm truncate">{waveform.api_name}</div>
        </button>
      {/each}
    </div>
  </section>

  <Separator class="mb-12" />

  <!-- Best Practices -->
  <section>
    <h2 class="text-2xl font-semibold mb-6">Best Practices</h2>

    <div class="grid sm:grid-cols-2 gap-4">
      <div class="p-4 rounded-lg border border-border bg-card space-y-2">
        <h3 class="font-medium">Use sparingly</h3>
        <p class="text-sm text-muted-foreground">
          Haptic feedback is most effective when used for key interactions, not
          every mouse movement.
        </p>
      </div>

      <div class="p-4 rounded-lg border border-border bg-card space-y-2">
        <h3 class="font-medium">Match the waveform to the action</h3>
        <p class="text-sm text-muted-foreground">
          Use subtle waveforms for hover states, sharp ones for button clicks,
          and alerts for notifications.
        </p>
      </div>

      <div class="p-4 rounded-lg border border-border bg-card space-y-2">
        <h3 class="font-medium">Handle disconnection gracefully</h3>
        <p class="text-sm text-muted-foreground">
          Not all users will have the plugin. Make haptics optional and don't
          break your app if the connection fails.
        </p>
      </div>

      <div class="p-4 rounded-lg border border-border bg-card space-y-2">
        <h3 class="font-medium">Prefer WebSocket for real-time</h3>
        <p class="text-sm text-muted-foreground">
          For slider drags, game feedback, or high-frequency events, use
          WebSocket for lower latency.
        </p>
      </div>
    </div>
  </section>
</div>
