<script lang="ts">
  import {
    Alert,
    AlertDescription,
    AlertTitle,
  } from "$lib/components/ui/alert";
  import { API_PORT } from "$lib/config";
  import {
    destroyHapticsWebSocket,
    getConnectionState,
    getErrorMessage,
    getErrorType,
    initHapticsWebSocket,
  } from "$lib/haptics.svelte";
  import AlertCircle from "@lucide/svelte/icons/alert-circle";
  import CheckCircle from "@lucide/svelte/icons/check-circle";
  import Loader2 from "@lucide/svelte/icons/loader-2";
  import Mouse from "@lucide/svelte/icons/mouse";
  import ShieldAlert from "@lucide/svelte/icons/shield-alert";
  import WifiOff from "@lucide/svelte/icons/wifi-off";
  import { onDestroy, onMount } from "svelte";

  onMount(() => {
    initHapticsWebSocket();
  });

  onDestroy(() => {
    destroyHapticsWebSocket();
  });

  const connectionState = $derived(getConnectionState());
  const errorType = $derived(getErrorType());
  const errorMessage = $derived(getErrorMessage());

  const statusConfig = $derived.by(() => {
    switch (connectionState) {
      case "connected":
        return {
          icon: CheckCircle,
          title: "Connected",
          description: "WebSocket connected to haptics service",
          borderClass: "border-green-500/30",
          bgClass: "bg-green-500/10",
          titleClass: "text-green-400",
          descClass: "text-green-300/80",
          iconClass: "text-green-400",
        };
      case "connecting":
      case "reconnecting":
        return {
          icon: Loader2,
          title:
            connectionState === "connecting"
              ? "Connecting..."
              : "Reconnecting...",
          description: "Establishing connection to haptics service",
          borderClass: "border-amber-500/30",
          bgClass: "bg-amber-500/10",
          titleClass: "text-amber-400",
          descClass: "text-amber-300/80",
          iconClass: "text-amber-400 animate-spin",
        };
      case "disconnected":
        switch (errorType) {
          case "connection_refused":
            return {
              icon: WifiOff,
              title: "Plugin Not Running",
              description:
                "Install the HapticWebPlugin and ensure Logi Options+ is running",
              borderClass: "border-red-500/30",
              bgClass: "bg-red-500/10",
              titleClass: "text-red-400",
              descClass: "text-red-300/80",
              iconClass: "text-red-400",
            };
          case "ssl_error":
            return {
              icon: ShieldAlert,
              title: "SSL Certificate Error",
              description:
                errorMessage ||
                "Trust the local.jmw.nz certificate in your browser",
              borderClass: "border-orange-500/30",
              bgClass: "bg-orange-500/10",
              titleClass: "text-orange-400",
              descClass: "text-orange-300/80",
              iconClass: "text-orange-400",
            };
          case "wrong_service":
            return {
              icon: AlertCircle,
              title: "Wrong Service",
              description:
                errorMessage ||
                `Another service is running on port ${API_PORT}`,
              borderClass: "border-red-500/30",
              bgClass: "bg-red-500/10",
              titleClass: "text-red-400",
              descClass: "text-red-300/80",
              iconClass: "text-red-400",
            };
          case "timeout":
            return {
              icon: AlertCircle,
              title: "Connection Timeout",
              description: "The haptics service is not responding",
              borderClass: "border-amber-500/30",
              bgClass: "bg-amber-500/10",
              titleClass: "text-amber-400",
              descClass: "text-amber-300/80",
              iconClass: "text-amber-400",
            };
          default:
            return {
              icon: WifiOff,
              title: "Disconnected",
              description: errorMessage || "Not connected to haptics service",
              borderClass: "border-gray-500/30",
              bgClass: "bg-gray-500/10",
              titleClass: "text-gray-400",
              descClass: "text-gray-300/80",
              iconClass: "text-gray-400",
            };
        }
    }
  });
</script>

<div class="flex flex-col sm:flex-row gap-3">
  <Alert class="{statusConfig.borderClass} {statusConfig.bgClass}">
    <statusConfig.icon class="h-4 w-4 {statusConfig.iconClass}" />
    <AlertTitle class={statusConfig.titleClass}
      >{statusConfig.title}</AlertTitle>
    <AlertDescription class={statusConfig.descClass}>
      {statusConfig.description}
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
