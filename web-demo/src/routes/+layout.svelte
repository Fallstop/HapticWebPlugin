<script lang="ts">
  import { TooltipProvider } from "$lib/components/ui/tooltip";
  import "./layout.css";

  let { children } = $props();
</script>

<TooltipProvider>
  {@render children()}
</TooltipProvider>
