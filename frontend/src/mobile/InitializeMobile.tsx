import { PaletteDark } from "../styles/Palette";

export async function InitializeMobile(): Promise<void> {
  const { Capacitor } = await import("@capacitor/core");
  if (Capacitor.getPlatform() === "web") {
    return;
  }

  // StatusBar: prevent overlay (content will layout below the status bar)
  const { StatusBar, Style } = await import("@capacitor/status-bar");
  await StatusBar.setOverlaysWebView({ overlay: false });
  await StatusBar.setBackgroundColor({
    color: PaletteDark.background?.paper ?? "#1E1E1E",
  });
  await StatusBar.setStyle({ style: Style.Dark });
}
