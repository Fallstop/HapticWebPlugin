<script lang="ts">
  import { Button } from "$lib/components/ui/button";
  import WaveformEncoding from "$lib/components/WaveformEncoding.svelte";
  import { triggerWaveform, WAVEFORMS } from "$lib/haptics";
  import Play from "@lucide/svelte/icons/play";
  import Shuffle from "@lucide/svelte/icons/shuffle";
  import Square from "@lucide/svelte/icons/square";
  import Trash2 from "@lucide/svelte/icons/trash-2";

  // Use waveforms that work well for rhythm
  const RHYTHM_WAVEFORMS = WAVEFORMS.filter((w) =>
    [
      "knock",
      "jingle",
      "sharp_collision",
      "subtle_collision",
      "completed",
    ].includes(w.api_name)
  );

  let isPlaying = $state(false);
  let currentBeat = $state(-1);
  let pattern = $state<boolean[]>([
    false,
    false,
    false,
    false,
    false,
    false,
    false,
    false,
  ]);
  let tempo = $state(120);
  let selectedWaveformApiName = $state("knock");
  let intervalId: ReturnType<typeof setInterval>;

  function getSelectedRhythmWaveform() {
    return (
      RHYTHM_WAVEFORMS.find((w) => w.api_name === selectedWaveformApiName) ||
      RHYTHM_WAVEFORMS[0]
    );
  }

  function toggleBeat(index: number) {
    pattern[index] = !pattern[index];
    if (pattern[index]) {
      triggerWaveform(selectedWaveformApiName);
    }
  }

  function startPlayback() {
    if (isPlaying) return;
    isPlaying = true;
    currentBeat = -1;

    const beatDuration = 60000 / tempo / 2; // 8th notes

    intervalId = setInterval(() => {
      currentBeat = (currentBeat + 1) % 8;
      if (pattern[currentBeat]) {
        triggerWaveform(selectedWaveformApiName);
      }
    }, beatDuration);
  }

  function stopPlayback() {
    isPlaying = false;
    currentBeat = -1;
    if (intervalId) {
      clearInterval(intervalId);
    }
  }

  function randomizePattern() {
    pattern = pattern.map(() => Math.random() > 0.5);
  }

  function clearPattern() {
    pattern = pattern.map(() => false);
  }
</script>

<div
  class="flex flex-col gap-4 p-6 rounded-xl bg-muted/30 border border-border">
  <div class="flex items-center justify-between">
    <div>
      <h3 class="text-lg font-semibold">Rhythm Sequencer</h3>
      <p class="text-sm text-muted-foreground">
        Create your own haptic beat pattern
      </p>
    </div>
  </div>

  <!-- Pattern Grid -->
  <div class="flex gap-2 justify-center">
    {#each pattern as active, i}
      <button
        class="w-10 h-10 rounded-lg border-2 transition-all duration-150
					{active
          ? 'bg-primary border-primary shadow-lg shadow-primary/30'
          : 'bg-muted/50 border-border hover:border-primary/50'}
					{currentBeat === i
          ? 'scale-110 ring-2 ring-primary ring-offset-2 ring-offset-background'
          : ''}"
        onclick={() => toggleBeat(i)}>
        {#if active}
          <span class="text-primary-foreground">●</span>
        {/if}
      </button>
    {/each}
  </div>

  <!-- Beat indicators -->
  <div class="flex gap-2 justify-center text-xs text-muted-foreground">
    {#each pattern as _, i}
      <span class="w-10 text-center">{i + 1}</span>
    {/each}
  </div>

  <!-- Controls -->
  <div class="flex flex-wrap items-center justify-center gap-3">
    <div class="flex items-center gap-2">
      <label for="tempo-input" class="text-sm text-muted-foreground"
        >Tempo:</label>
      <input
        id="tempo-input"
        type="number"
        bind:value={tempo}
        min="60"
        max="200"
        class="w-16 px-2 py-1 rounded bg-muted border border-border text-sm"
        disabled={isPlaying} />
      <span class="text-xs text-muted-foreground">BPM</span>
    </div>

    <div class="flex items-center gap-2">
      <label for="waveform-select" class="text-sm text-muted-foreground"
        >Sound:</label>
      <select
        id="waveform-select"
        bind:value={selectedWaveformApiName}
        class="px-3 py-1 rounded bg-muted border border-border text-sm"
        disabled={isPlaying}>
        {#each RHYTHM_WAVEFORMS as wf}
          <option value={wf.api_name}>
            {wf.name}
          </option>
        {/each}
      </select>
    </div>
  </div>

  <!-- Selected waveform preview -->
  <div
    class="flex items-center justify-center gap-3 p-2 rounded bg-background/50">
    <span class="text-xs text-muted-foreground">Pattern:</span>
    <WaveformEncoding
      encoding={getSelectedRhythmWaveform().waveform_encoding}
      size="sm" />
  </div>

  <div class="flex justify-center gap-2">
    {#if isPlaying}
      <Button variant="destructive" onclick={stopPlayback}>
        <Square class="w-4 h-4 mr-1" /> Stop
      </Button>
    {:else}
      <Button onclick={startPlayback}>
        <Play class="w-4 h-4 mr-1" /> Play
      </Button>
    {/if}
    <Button variant="outline" onclick={randomizePattern} disabled={isPlaying}>
      <Shuffle class="w-4 h-4 mr-1" /> Random
    </Button>
    <Button variant="ghost" onclick={clearPattern} disabled={isPlaying}>
      <Trash2 class="w-4 h-4 mr-1" /> Clear
    </Button>
  </div>
</div>
