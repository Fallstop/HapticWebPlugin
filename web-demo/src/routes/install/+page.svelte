<script lang="ts">
  import { Button } from "$lib/components/ui/button";
  import { Separator } from "$lib/components/ui/separator";
  import AlertTriangle from "@lucide/svelte/icons/alert-triangle";
  import Download from "@lucide/svelte/icons/download";
  import ExternalLink from "@lucide/svelte/icons/external-link";
  import Folder from "@lucide/svelte/icons/folder";
  import Trash2 from "@lucide/svelte/icons/trash-2";
</script>

<svelte:head>
  <title>Install — HapticWeb</title>
  <meta
    name="description"
    content="Install the HapticWeb plugin for MX Master 4 haptic feedback." />
</svelte:head>

<div class="max-w-6xl mx-auto px-4 py-12">
  <!-- Header -->
  <div class="space-y-4 mb-12">
    <h1 class="text-4xl font-bold">Install HapticWeb Plugin</h1>
    <p class="text-lg text-muted-foreground">
      Get the plugin running in under a minute. Just download, double-click, and
      you're ready.
    </p>
  </div>

  <!-- Requirements -->
  <section class="mb-12">
    <h2 class="text-2xl font-semibold mb-6">Requirements</h2>
    <div class="grid sm:grid-cols-2 gap-4">
      <div class="p-4 rounded-lg border border-border bg-card flex gap-4">
        <img
          src="/mx-master4.webp"
          alt="MX Master 4"
          class="w-16 h-16 object-contain" />
        <div>
          <h3 class="font-medium">Logitech MX Master 4</h3>
          <p class="text-sm text-muted-foreground">
            The only mouse with controllable haptics via Actions SDK
          </p>
        </div>
      </div>
      <div class="p-4 rounded-lg border border-border bg-card flex gap-4">
        <img
          src="/options-plus.png"
          alt="Logi Options+"
          class="w-16 h-16 object-contain" />
        <div>
          <h3 class="font-medium">Logi Options+</h3>
          <p class="text-sm text-muted-foreground">
            Logitech's device management software (free)
          </p>
          <a
            href="https://www.logitech.com/software/logi-options-plus.html"
            target="_blank"
            rel="noopener noreferrer"
            class="inline-flex items-center gap-1 text-sm text-primary hover:underline mt-2">
            Download Logi Options+ <ExternalLink class="w-3 h-3" />
          </a>
        </div>
      </div>
    </div>
  </section>

  <Separator class="mb-12" />

  <!-- Installation Steps -->
  <section class="mb-12">
    <h2 class="text-2xl font-semibold mb-6">Installation</h2>

    <div class="space-y-6">
      <!-- Step 1 -->
      <div class="flex gap-4">
        <div
          class="shrink-0 w-8 h-8 rounded-full bg-primary/20 flex items-center justify-center text-primary font-semibold">
          1
        </div>
        <div class="flex-1 space-y-3">
          <h3 class="font-medium">Download the plugin</h3>
          <p class="text-sm text-muted-foreground">
            Get the latest
            <code class="px-1.5 py-0.5 rounded bg-muted text-foreground">
              HapticWeb.lplug4
            </code>
            file from GitHub Releases.
          </p>
          <Button
            href="https://github.com/fallstop/HapticWebPlugin/releases/latest/download/HapticWeb.lplug4"
            variant="outline"
            size="sm">
            <Download class="w-4 h-4 mr-2" />
            Download HapticWeb.lplug4
          </Button>
        </div>
      </div>

      <!-- Step 2 -->
      <div class="flex gap-4">
        <div
          class="shrink-0 w-8 h-8 rounded-full bg-primary/20 flex items-center justify-center text-primary font-semibold">
          2
        </div>
        <div class="flex-1 space-y-3">
          <h3 class="font-medium">Open plugin management in Logi Options+</h3>
          <p class="text-sm text-muted-foreground">
            Open Logi Options+ → Click on your MX Master 4 → Go to <strong
              >HAPTIC FEEDBACK</strong>
            in the sidebar → Click the settings popover → Click
            <strong>INSTALL AND UNINSTALL PLUGINS</strong>.
          </p>
        </div>
      </div>

      <!-- Step 3 -->
      <div class="flex gap-4">
        <div
          class="shrink-0 w-8 h-8 rounded-full bg-primary/20 flex items-center justify-center text-primary font-semibold">
          3
        </div>
        <div class="flex-1 space-y-3">
          <h3 class="font-medium">Install the plugin</h3>
          <p class="text-sm text-muted-foreground">
            Double-click the downloaded <code
              class="px-1.5 py-0.5 rounded bg-muted text-foreground"
              >HapticWeb.lplug4</code>
            file. Logi Options+ will show an installation dialog — click
            <strong>Continue</strong>.
          </p>
        </div>
      </div>

      <!-- Step 4 -->
      <div class="flex gap-4">
        <div
          class="shrink-0 w-8 h-8 rounded-full bg-green-500/20 flex items-center justify-center text-green-500 font-semibold">
          4
        </div>
        <div class="flex-1 space-y-3">
          <h3 class="font-medium">Done!</h3>
          <p class="text-sm text-muted-foreground">
            The server starts automatically at <code
              class="px-1.5 py-0.5 rounded bg-muted text-foreground"
              >https://local.jmw.nz:41443/</code
            >. Test it on the
            <a href="/playground" class="text-primary hover:underline"
              >Playground</a
            >.
          </p>
        </div>
      </div>
    </div>
  </section>

  <Separator class="mb-12" />

  <!-- About local.jmw.nz -->
  <section class="mb-12">
    <h2 class="text-2xl font-semibold mb-6">About local.jmw.nz</h2>
    <div class="p-4 rounded-lg border border-border bg-card space-y-3">
      <p class="text-sm text-muted-foreground">
        <code class="px-1.5 py-0.5 rounded bg-muted text-foreground"
          >local.jmw.nz</code>
        resolves to
        <code class="px-1.5 py-0.5 rounded bg-muted text-foreground"
          >127.0.0.1</code> (localhost).
      </p>
      <p class="text-sm text-muted-foreground">
        The plugin uses this domain to serve HTTPS with a valid SSL certificate
        instead of self-signed certs that browsers reject. The certificate is
        automatically downloaded from GitHub, cached locally, and refreshed
        every 24 hours.
      </p>
    </div>
  </section>

  <Separator class="mb-12" />

  <!-- Uninstalling -->
  <section class="mb-12">
    <h2 class="text-2xl font-semibold mb-6 flex items-center gap-2">
      <Trash2 class="w-5 h-5" />
      Uninstalling
    </h2>
    <p class="text-muted-foreground mb-4">
      Manually installed plugins don't appear in Logi Options+ plugin list. To
      uninstall, delete the plugin folder:
    </p>

    <div class="space-y-3">
      <div class="p-3 rounded-lg bg-muted font-mono text-sm">
        <div class="flex items-center gap-2 text-muted-foreground mb-1">
          <Folder class="w-4 h-4" />
          <span class="font-sans text-xs">Windows</span>
        </div>
        C:\Users\USERNAME\AppData\Local\Logi\LogiPluginService\Plugins\HapticWebPlugin\
      </div>

      <div class="p-3 rounded-lg bg-muted font-mono text-sm">
        <div class="flex items-center gap-2 text-muted-foreground mb-1">
          <Folder class="w-4 h-4" />
          <span class="font-sans text-xs">macOS</span>
        </div>
        ~/Library/Application Support/Logi/LogiPluginService/Plugins/HapticWebPlugin/
      </div>
    </div>
  </section>

  <Separator class="mb-12" />

  <!-- Troubleshooting -->
  <section>
    <h2 class="text-2xl font-semibold mb-6 flex items-center gap-2">
      <AlertTriangle class="w-5 h-5" />
      Troubleshooting
    </h2>

    <div class="space-y-6">
      <div class="space-y-2">
        <h3 class="font-medium">Certificate errors</h3>
        <ul
          class="text-sm text-muted-foreground space-y-1 list-disc list-inside">
          <li>
            Check plugin status in Logi Options+ (should show green "Normal")
          </li>
          <li>
            Verify internet connection (initial certificate download requires
            network)
          </li>
          <li>Restart Logi Plugin Service</li>
          <li>Report an issue with plugin logs if the problem persists</li>
        </ul>
      </div>

      <div class="space-y-2">
        <h3 class="font-medium">Port already in use</h3>
        <p class="text-sm text-muted-foreground">
          If port 41443 is in use, stop other services using the port:
        </p>
        <div class="p-3 rounded-lg bg-muted font-mono text-sm">
          lsof -ti:41443 | xargs kill -9
        </div>
        <p class="text-sm text-muted-foreground">
          Then restart Logi Plugin Service.
        </p>
      </div>

      <div class="space-y-2">
        <h3 class="font-medium">WebSocket connection failed</h3>
        <ul
          class="text-sm text-muted-foreground space-y-1 list-disc list-inside">
          <li>
            Verify the server is running: <code
              class="px-1 py-0.5 rounded bg-muted text-foreground"
              >curl https://local.jmw.nz:41443/</code>
          </li>
          <li>Check browser console for errors</li>
          <li>
            Ensure you're using <code
              class="px-1 py-0.5 rounded bg-muted text-foreground">wss://</code>
            (not
            <code class="px-1 py-0.5 rounded bg-muted text-foreground"
              >ws://</code
            >)
          </li>
          <li>
            Verify <code class="px-1 py-0.5 rounded bg-muted text-foreground"
              >local.jmw.nz</code>
            resolves to
            <code class="px-1 py-0.5 rounded bg-muted text-foreground"
              >127.0.0.1</code>
          </li>
        </ul>
      </div>
    </div>
  </section>
</div>
