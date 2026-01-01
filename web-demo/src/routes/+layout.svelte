<script lang="ts">
  import favicon from "$lib/assets/favicon.svg";
  import { TooltipProvider } from "$lib/components/ui/tooltip";
  import "./layout.css";

  let { children } = $props();
</script>

<svelte:head><link rel="icon" href={favicon} /></svelte:head>
<TooltipProvider>
  {@render children()}
</TooltipProvider>
