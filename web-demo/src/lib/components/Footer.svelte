<script lang="ts">
  import Github from "@lucide/svelte/icons/github";
  import Scale from "@lucide/svelte/icons/scale";
</script>

<footer class="border-t border-border mt-16">
  <div class="max-w-6xl mx-auto px-4 py-8">
    <div class="flex flex-col sm:flex-row items-center justify-between gap-4">
      <p class="text-sm text-muted-foreground">
        Haptic Web Plugin - Made by <a
          href="https://jmw.nz"
          target="_blank"
          class="text-primary hover:underline">Jasper M-W</a>
      </p>

      <div class="flex items-center gap-6">
        <a
          href="https://github.com/fallstop/HapticWebPlugin/blob/main/LICENSE.md"
          target="_blank"
          rel="noopener noreferrer"
          class="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <Scale class="w-4 h-4" />
          <span>MIT License</span>
        </a>

        <a
          href="https://github.com/fallstop/HapticWebPlugin"
          target="_blank"
          rel="noopener noreferrer"
          class="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <Github class="w-4 h-4" />
          <span>GitHub</span>
        </a>
      </div>
    </div>
  </div>
</footer>
