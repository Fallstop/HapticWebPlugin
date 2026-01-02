<script lang="ts">
  import Footer from "$lib/components/Footer.svelte";
  import Navigation from "$lib/components/Navigation.svelte";
  import { TooltipProvider } from "$lib/components/ui/tooltip";
  import { destroy } from "$lib/connection.svelte";
  import { onDestroy } from "svelte";
  import "./layout.css";

  let { children } = $props();

  onDestroy(() => {
    destroy();
  });
</script>

<TooltipProvider>
  <div class="min-h-screen bg-background flex flex-col">
    <Navigation />
    <main class="flex-1">
      {@render children()}
    </main>
    <Footer />
  </div>
</TooltipProvider>
