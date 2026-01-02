<script lang="ts">
  import { Badge } from "$lib/components/ui/badge";
  import { Separator } from "$lib/components/ui/separator";
  import { API_BASE_URL, API_HOST, API_PORT } from "$lib/config";
  import { WAVEFORMS } from "$lib/haptics";
  import Check from "@lucide/svelte/icons/check";
  import Copy from "@lucide/svelte/icons/copy";

  let copiedIndex = $state<number | null>(null);

  async function copyToClipboard(text: string, index: number) {
    await navigator.clipboard.writeText(text);
    copiedIndex = index;
    setTimeout(() => {
      copiedIndex = null;
    }, 2000);
  }
</script>

<svelte:head>
  <title>API Reference — HapticWeb</title>
  <meta
    name="description"
    content="Complete API documentation for the HapticWeb plugin." />
</svelte:head>

<div class="max-w-6xl mx-auto px-4 py-12">
  <!-- Header -->
  <div class="space-y-4 mb-12">
    <h1 class="text-4xl font-bold">API Documentation</h1>
    <p class="text-lg text-muted-foreground">
      Complete reference for the REST and WebSocket APIs.
    </p>
  </div>

  <!-- Base URL -->
  <section class="mb-12">
    <h2 class="text-2xl font-semibold mb-4">Base URL</h2>
    <div class="p-4 rounded-lg bg-muted font-mono text-sm">
      {API_BASE_URL}
    </div>
    <p class="text-sm text-muted-foreground mt-2">
      All REST endpoints are served over HTTPS. The server binds to <code
        class="px-1 py-0.5 rounded bg-muted text-foreground">127.0.0.1</code> only.
    </p>
  </section>

  <Separator class="mb-12" />

  <!-- REST Endpoints -->
  <section class="mb-12">
    <h2 class="text-2xl font-semibold mb-6">REST Endpoints</h2>

    <div class="space-y-8">
      <!-- GET / -->
      <div class="space-y-4">
        <div class="flex items-center gap-3">
          <Badge class="bg-green-500/20 text-green-400 border-green-500/30"
            >GET</Badge>
          <code class="font-mono">/</code>
        </div>
        <p class="text-muted-foreground">
          Health check. Returns service info and available endpoints.
        </p>

        <div class="relative">
          <pre
            class="bg-muted/50 rounded-lg p-4 pr-12 overflow-x-auto text-sm font-mono">curl {API_BASE_URL}/</pre>
          <button
            onclick={() => copyToClipboard(`curl ${API_BASE_URL}/`, 0)}
            class="absolute top-3 right-3 p-1.5 rounded-md hover:bg-muted transition-colors">
            {#if copiedIndex === 0}
              <Check class="h-4 w-4 text-green-400" />
            {:else}
              <Copy class="h-4 w-4 text-muted-foreground" />
            {/if}
          </button>
        </div>

        <details>
          <summary
            class="text-xs text-muted-foreground cursor-pointer hover:text-foreground"
            >Response</summary>
          <pre
            class="mt-2 bg-muted/30 rounded-lg p-4 text-xs font-mono text-muted-foreground overflow-x-auto">{`{
  "success": true,
  "service": "HapticWebPlugin",
  "version": "1.0.0",
  "endpoints": {
    "/": "Health check",
    "/waveforms": "List available waveforms",
    "/haptic/{waveform}": "Trigger haptic (POST)",
    "/ws": "WebSocket endpoint"
  }
}`}</pre>
        </details>
      </div>

      <Separator />

      <!-- GET /waveforms -->
      <div class="space-y-4">
        <div class="flex items-center gap-3">
          <Badge class="bg-green-500/20 text-green-400 border-green-500/30"
            >GET</Badge>
          <code class="font-mono">/waveforms</code>
        </div>
        <p class="text-muted-foreground">
          List all available haptic waveforms.
        </p>

        <div class="relative">
          <pre
            class="bg-muted/50 rounded-lg p-4 pr-12 overflow-x-auto text-sm font-mono">curl {API_BASE_URL}/waveforms</pre>
          <button
            onclick={() => copyToClipboard(`curl ${API_BASE_URL}/waveforms`, 1)}
            class="absolute top-3 right-3 p-1.5 rounded-md hover:bg-muted transition-colors">
            {#if copiedIndex === 1}
              <Check class="h-4 w-4 text-green-400" />
            {:else}
              <Copy class="h-4 w-4 text-muted-foreground" />
            {/if}
          </button>
        </div>

        <details>
          <summary
            class="text-xs text-muted-foreground cursor-pointer hover:text-foreground"
            >Response</summary>
          <pre
            class="mt-2 bg-muted/30 rounded-lg p-4 text-xs font-mono text-muted-foreground overflow-x-auto">{`{
  "success": true,
  "count": 16,
  "waveforms": [
    "sharp_collision",
    "sharp_state_change",
    "knock",
    ...
  ]
}`}</pre>
        </details>
      </div>

      <Separator />

      <!-- POST /haptic/{waveform} -->
      <div class="space-y-4">
        <div class="flex items-center gap-3">
          <Badge class="bg-blue-500/20 text-blue-400 border-blue-500/30"
            >POST</Badge>
          <code class="font-mono">/haptic/{"{waveform}"}</code>
        </div>
        <p class="text-muted-foreground">
          Trigger a specific haptic waveform by name.
        </p>

        <div class="relative">
          <pre
            class="bg-muted/50 rounded-lg p-4 pr-12 overflow-x-auto text-sm font-mono">curl -X POST {API_BASE_URL}/haptic/knock -d ''</pre>
          <button
            onclick={() =>
              copyToClipboard(
                `curl -X POST ${API_BASE_URL}/haptic/knock -d ''`,
                2
              )}
            class="absolute top-3 right-3 p-1.5 rounded-md hover:bg-muted transition-colors">
            {#if copiedIndex === 2}
              <Check class="h-4 w-4 text-green-400" />
            {:else}
              <Copy class="h-4 w-4 text-muted-foreground" />
            {/if}
          </button>
        </div>

        <div class="p-3 rounded-lg bg-amber-500/10 border border-amber-500/30">
          <p class="text-sm text-amber-200">
            <strong>Note:</strong> The
            <code class="px-1 py-0.5 rounded bg-muted text-foreground"
              >-d ''</code>
            is required to send a
            <code class="px-1 py-0.5 rounded bg-muted text-foreground"
              >Content-Length</code> header. Without it, curl will wait indefinitely.
          </p>
        </div>

        <details>
          <summary
            class="text-xs text-muted-foreground cursor-pointer hover:text-foreground"
            >Response</summary>
          <pre
            class="mt-2 bg-muted/30 rounded-lg p-4 text-xs font-mono text-muted-foreground overflow-x-auto">{`{
  "success": true,
  "waveform": "knock"
}`}</pre>
        </details>
      </div>
    </div>
  </section>

  <Separator class="mb-12" />

  <!-- WebSocket -->
  <section class="mb-12">
    <h2 class="text-2xl font-semibold mb-6">WebSocket API</h2>

    <div class="space-y-4">
      <div class="p-4 rounded-lg bg-muted">
        <p class="font-mono text-sm">wss://{API_HOST}:{API_PORT}/ws</p>
      </div>

      <div class="space-y-2">
        <h3 class="font-medium">Protocol</h3>
        <ul
          class="text-sm text-muted-foreground space-y-1 list-disc list-inside">
          <li>Send a single byte containing the waveform index (0-15)</li>
          <li>No response is sent back</li>
          <li>Connection stays open for repeated triggers</li>
          <li>
            Binary message type (<code
              class="px-1 py-0.5 rounded bg-muted text-foreground"
              >Uint8Array</code
            >)
          </li>
        </ul>
      </div>

      <div class="relative">
        <pre
          class="bg-muted/50 rounded-lg p-4 pr-12 overflow-x-auto text-sm font-mono">{`const ws = new WebSocket('wss://${API_HOST}:${API_PORT}/ws');

ws.onopen = () => {
  // Trigger waveform at index 0 (sharp_collision)
  ws.send(new Uint8Array([0]));
};`}</pre>
        <button
          onclick={() =>
            copyToClipboard(
              `const ws = new WebSocket('wss://${API_HOST}:${API_PORT}/ws');\n\nws.onopen = () => {\n  // Trigger waveform at index 0 (sharp_collision)\n  ws.send(new Uint8Array([0]));\n};`,
              3
            )}
          class="absolute top-3 right-3 p-1.5 rounded-md hover:bg-muted transition-colors">
          {#if copiedIndex === 3}
            <Check class="h-4 w-4 text-green-400" />
          {:else}
            <Copy class="h-4 w-4 text-muted-foreground" />
          {/if}
        </button>
      </div>
    </div>
  </section>

  <Separator class="mb-12" />

  <!-- Waveform Table -->
  <section>
    <h2 class="text-2xl font-semibold mb-6">Available Waveforms</h2>
    <p class="text-muted-foreground mb-4">
      16 waveforms across different categories. Use the index for WebSocket, or
      the API name for REST.
    </p>

    <div class="overflow-x-auto">
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-border">
            <th class="text-left py-3 px-4 font-medium">Index</th>
            <th class="text-left py-3 px-4 font-medium">API Name</th>
            <th class="text-left py-3 px-4 font-medium">Category</th>
          </tr>
        </thead>
        <tbody>
          {#each WAVEFORMS as waveform, index}
            <tr class="border-b border-border/50 hover:bg-muted/30">
              <td class="py-3 px-4 font-mono text-muted-foreground">{index}</td>
              <td class="py-3 px-4 font-mono">{waveform.api_name}</td>
              <td class="py-3 px-4">
                <Badge variant="outline" class="text-xs"
                  >{waveform.category}</Badge>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>
  </section>
</div>
