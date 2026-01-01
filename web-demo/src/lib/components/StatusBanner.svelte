<script lang="ts">
  import {
    Alert,
    AlertDescription,
    AlertTitle,
  } from "$lib/components/ui/alert";
  import MonitorSmartphone from "@lucide/svelte/icons/monitor-smartphone";
  import Mouse from "@lucide/svelte/icons/mouse";
</script>

<div class="flex flex-col sm:flex-row gap-3">
  <Alert class="border-amber-500/30 bg-amber-500/10">
    <MonitorSmartphone class="h-4 w-4 text-amber-400" />
    <AlertTitle class="text-amber-400">Desktop Only</AlertTitle>
    <AlertDescription class="text-amber-300/80">
      This demo requires the plugin running on localhost:8765
    </AlertDescription>
  </Alert>

  <Alert class="border-blue-500/30 bg-blue-500/10">
    <Mouse class="h-4 w-4 text-blue-400" />
    <AlertTitle class="text-blue-400">Logi Options+ Required</AlertTitle>
    <AlertDescription class="text-blue-300/80">
      MX Master 4 with Logi Options+ and the haptics plugin installed
    </AlertDescription>
  </Alert>
</div>
