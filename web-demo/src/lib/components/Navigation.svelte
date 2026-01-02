<script lang="ts">
  import { page } from "$app/stores";
  import { Button } from "$lib/components/ui/button";
  import {
    connect,
    hasInitiatedConnection,
    isConnected,
  } from "$lib/connection.svelte";
  import Github from "@lucide/svelte/icons/github";
  import Menu from "@lucide/svelte/icons/menu";
  import Radio from "@lucide/svelte/icons/radio";
  import Vibrate from "@lucide/svelte/icons/vibrate";
  import WifiOff from "@lucide/svelte/icons/wifi-off";
  import X from "@lucide/svelte/icons/x";

  let mobileMenuOpen = $state(false);

  const connected = $derived(isConnected());
  const hasInitiated = $derived(hasInitiatedConnection());

  const navLinks = [
    { href: "/", label: "Home" },
    { href: "/install", label: "Install" },
    { href: "/integrate", label: "Integrate" },
    { href: "/api", label: "API" },
    { href: "/playground", label: "Playground" },
  ];

  function handleConnect() {
    connect();
  }

  const connectionStatus = $derived.by(() => {
    if (connected) {
      return { icon: Radio, class: "text-green-400", label: "Connected" };
    }
    if (hasInitiated) {
      return { icon: WifiOff, class: "text-red-400", label: "Disconnected" };
    }
    return null;
  });
</script>

<nav
  class="sticky top-0 z-50 border-b border-border bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60">
  <div class="max-w-6xl mx-auto px-4">
    <div class="flex h-16 items-center justify-between">
      <!-- Logo -->
      <a href="/" class="flex items-center gap-2 font-semibold">
        <div class="p-1.5 rounded-lg bg-primary/20 border border-primary/30">
          <Vibrate class="w-4 h-4 text-primary" />
        </div>
        <span class="hidden sm:inline">HapticWeb</span>
      </a>

      <!-- Desktop nav -->
      <div class="hidden md:flex items-center gap-1">
        {#each navLinks as link}
          <a
            href={link.href}
            class="px-3 py-2 text-sm rounded-md transition-colors {$page.url
              .pathname === link.href
              ? 'text-primary font-medium'
              : 'text-muted-foreground hover:text-foreground hover:bg-accent'}">
            {link.label}
          </a>
        {/each}
      </div>

      <!-- Right side -->
      <div class="flex items-center gap-3">
        <!-- Connection status/button -->
        {#if connectionStatus}
          <div
            class="hidden sm:flex items-center gap-1.5 text-sm {connectionStatus.class}">
            <connectionStatus.icon class="w-4 h-4" />
            <span>{connectionStatus.label}</span>
          </div>
        {:else}
          <Button
            size="sm"
            variant="outline"
            onclick={handleConnect}
            class="hidden sm:flex">
            Connect Mouse
          </Button>
        {/if}

        <!-- GitHub -->
        <a
          href="https://github.com/fallstop/HapticWebPlugin"
          target="_blank"
          rel="noopener noreferrer"
          class="p-2 rounded-md text-muted-foreground hover:text-foreground hover:bg-accent transition-colors">
          <Github class="w-5 h-5" />
        </a>

        <!-- Mobile menu button -->
        <button
          onclick={() => (mobileMenuOpen = !mobileMenuOpen)}
          class="md:hidden p-2 rounded-md text-muted-foreground hover:text-foreground hover:bg-accent">
          {#if mobileMenuOpen}
            <X class="w-5 h-5" />
          {:else}
            <Menu class="w-5 h-5" />
          {/if}
        </button>
      </div>
    </div>

    <!-- Mobile menu -->
    {#if mobileMenuOpen}
      <div class="md:hidden pb-4 space-y-1">
        {#each navLinks as link}
          <a
            href={link.href}
            onclick={() => (mobileMenuOpen = false)}
            class="block px-3 py-2 text-sm rounded-md transition-colors {$page
              .url.pathname === link.href
              ? 'text-primary font-medium bg-primary/10'
              : 'text-muted-foreground hover:text-foreground hover:bg-accent'}">
            {link.label}
          </a>
        {/each}

        {#if !connectionStatus}
          <Button
            size="sm"
            variant="outline"
            onclick={handleConnect}
            class="w-full mt-2">
            Connect Mouse
          </Button>
        {:else}
          <div
            class="flex items-center gap-1.5 px-3 py-2 text-sm {connectionStatus.class}">
            <connectionStatus.icon class="w-4 h-4" />
            <span>{connectionStatus.label}</span>
          </div>
        {/if}
      </div>
    {/if}
  </div>
</nav>
