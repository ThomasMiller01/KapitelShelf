import { StatusBar, Style } from "@capacitor/status-bar";

import { PaletteDark } from "../styles/Palette";
import { IsMobileApp } from "../utils/MobileUtils";

export const InitializeMobile = async (): Promise<void> => {
  if (!IsMobileApp()) {
    return;
  }

  // StatusBar
  // prevent overlay (content will layout below the status bar)
  await StatusBar.setOverlaysWebView({ overlay: false });
  await StatusBar.setBackgroundColor({
    color: PaletteDark.background?.paper ?? "#1E1E1E",
  });
  await StatusBar.setStyle({ style: Style.Dark });
};
