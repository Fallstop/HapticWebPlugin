<script lang="ts">
  import { Badge } from "$lib/components/ui/badge";
  import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
  } from "$lib/components/ui/card";
  import { Separator } from "$lib/components/ui/separator";
  import { API_BASE_URL, API_HOST, API_PORT } from "$lib/config";
  import { WAVEFORMS } from "$lib/haptics";
  import { triggerHapticWs } from "$lib/haptics.svelte";
  import Check from "@lucide/svelte/icons/check";
  import Copy from "@lucide/svelte/icons/copy";
  import Terminal from "@lucide/svelte/icons/terminal";

  const API_BASE = API_BASE_URL;

  interface ApiExample {
    name: string;
    method: string;
    endpoint: string;
    description: string;
    curl: string;
    response?: string;
  }

  const examples: ApiExample[] = [
    {
      name: "Health Check",
      method: "GET",
      endpoint: "/",
      description:
        "Check if the haptics service is running and get service info",
      curl: `curl ${API_BASE}/`,
      response: `{
  "success": true,
  "service": "HapticWebPlugin",
  "version": "1.0.0",
  "endpoints": { ... }
}`,
    },
    {
      name: "List Waveforms",
      method: "GET",
      endpoint: "/waveforms",
      description: "Get all available haptic waveforms",
      curl: `curl ${API_BASE}/waveforms`,
      response: `{
  "success": true,
  "count": 16,
  "waveforms": ["sharp_collision", "knock", ...]
}`,
    },
    {
      name: "Trigger Haptic",
      method: "POST",
      endpoint: "/haptic/{waveform}",
      description: "Trigger a specific haptic waveform by name",
      curl: `curl -X POST ${API_BASE}/haptic/knock -d ''`,
      response: `{
  "success": true,
  "waveform": "knock"
}`,
    },
  ];

  let copiedIndex = $state<number | null>(null);

  async function copyToClipboard(text: string, index: number) {
    triggerHapticWs("damp_collision");
    await navigator.clipboard.writeText(text);
    copiedIndex = index;
    setTimeout(() => {
      copiedIndex = null;
    }, 2000);
  }

  function getMethodColor(method: string): string {
    switch (method) {
      case "GET":
        return "bg-green-500/20 text-green-400 border-green-500/30";
      case "POST":
        return "bg-blue-500/20 text-blue-400 border-blue-500/30";
      default:
        return "bg-gray-500/20 text-gray-400 border-gray-500/30";
    }
  }
</script>

<Card>
  <CardHeader>
    <div class="flex items-center gap-3">
      <Terminal class="h-5 w-5 text-primary" />
      <CardTitle>API Reference</CardTitle>
    </div>
    <CardDescription>
      REST API endpoints for integrating haptic feedback. The WebSocket endpoint
      <code class="px-1.5 py-0.5 rounded bg-muted font-mono text-xs"
        >wss://{API_HOST}:{API_PORT}/ws</code>
      provides lower latency for real-time triggering.
    </CardDescription>
  </CardHeader>
  <CardContent class="space-y-6">
    {#each examples as example, i}
      <div class="space-y-3">
        <div class="flex items-center gap-3">
          <Badge variant="outline" class={getMethodColor(example.method)}>
            {example.method}
          </Badge>
          <code class="font-mono text-sm text-muted-foreground"
            >{example.endpoint}</code>
        </div>

        <p class="text-sm text-muted-foreground">{example.description}</p>

        <div class="relative">
          <pre
            class="bg-muted/50 rounded-lg p-4 pr-12 overflow-x-auto text-sm font-mono">{example.curl}</pre>
          <button
            onclick={() => copyToClipboard(example.curl, i)}
            class="absolute top-3 right-3 p-1.5 rounded-md hover:bg-muted transition-colors"
            aria-label="Copy to clipboard">
            {#if copiedIndex === i}
              <Check class="h-4 w-4 text-green-400" />
            {:else}
              <Copy class="h-4 w-4 text-muted-foreground" />
            {/if}
          </button>
        </div>

        {#if example.response}
          <details class="group">
            <summary
              class="text-xs text-muted-foreground cursor-pointer hover:text-foreground transition-colors">
              Example response
            </summary>
            <pre
              class="mt-2 bg-muted/30 rounded-lg p-4 overflow-x-auto text-xs font-mono text-muted-foreground">{example.response}</pre>
          </details>
        {/if}

        {#if i < examples.length - 1}
          <Separator class="mt-4" />
        {/if}
      </div>
    {/each}

    <Separator />

    <div class="space-y-3">
      <h4 class="font-medium">Available Waveforms</h4>
      <div class="flex flex-wrap gap-2">
        {#each WAVEFORMS as waveform}
          <Badge variant="secondary" class="font-mono text-xs">
            {waveform.api_name}
          </Badge>
        {/each}
      </div>
    </div>
  </CardContent>
</Card>
