<script lang="ts">
  import { Button } from "$lib/components/ui/button";
  import { isConnected, triggerHaptic } from "$lib/connection.svelte";
  import RotateCcw from "@lucide/svelte/icons/rotate-ccw";

  let email = $state("");
  let submitted = $state(false);
  let isValid = $state(false);
  const connected = $derived(isConnected());

  function validateEmail(value: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
  }

  function handleSubmit() {
    if (!connected) return;

    const valid = validateEmail(email);
    submitted = true;
    isValid = valid;

    if (!valid) {
      triggerHaptic("mad");
    }
  }

  function handleReset() {
    email = "";
    submitted = false;
    isValid = false;
  }
</script>

<div
  class="flex flex-col gap-4 p-6 rounded-xl bg-muted/30 border border-border">
  <h3 class="text-lg font-semibold">Form Validation Feedback</h3>
  <p class="text-sm text-muted-foreground">
    Enter an email and submit. Invalid emails trigger an error haptic to
    reinforce the mistake.
  </p>

  {#if submitted && isValid}
    <!-- Success state - matches form height -->
    <div
      class="h-39 flex flex-col items-center justify-center gap-3 rounded-lg bg-green-500/10 border border-green-500/30">
      <div class="text-green-400 text-3xl">✓</div>
      <div class="text-center">
        <p class="font-medium text-green-300">Form submitted</p>
        <p class="text-xs text-muted-foreground mt-0.5 truncate max-w-[200px]">
          {email}
        </p>
      </div>
      <Button onclick={handleReset} variant="outline" size="sm">
        <RotateCcw class="w-3 h-3 mr-1.5" />
        Try Again
      </Button>
    </div>
  {:else}
    <!-- Form state -->
    <div class="h-39 flex flex-col justify-between">
      <div>
        <label for="email" class="text-sm font-medium mb-1.5 block"
          >Email</label>
        <input
          id="email"
          type="text"
          bind:value={email}
          disabled={!connected}
          placeholder="you@example.com"
          class="w-full px-3 py-2 rounded-lg border bg-background text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary/50 disabled:opacity-50 disabled:cursor-not-allowed {submitted &&
          !isValid
            ? 'border-red-500'
            : 'border-border'}" />
        <p
          class="text-xs mt-1 h-4 {submitted && !isValid
            ? 'text-red-400'
            : 'text-transparent'}">
          {submitted && !isValid ? "Please enter a valid email" : "\u00A0"}
        </p>
      </div>

      <div>
        <Button onclick={handleSubmit} disabled={!connected} class="w-full">
          Submit
        </Button>
        {#if !connected}
          <p class="text-xs text-muted-foreground text-center mt-2">
            Connect your mouse to enable
          </p>
        {:else}
          <p class="text-xs text-transparent text-center mt-2">&nbsp;</p>
        {/if}
      </div>
    </div>
  {/if}

  <div class="text-xs text-muted-foreground border-t border-border pt-3 mt-2">
    <span class="font-medium">Waveform used:</span>
    <span class="text-red-400">mad</span> (error)
  </div>
</div>
