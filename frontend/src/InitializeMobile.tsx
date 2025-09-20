import { App } from "@capacitor/app";
import { StatusBar, Style } from "@capacitor/status-bar";

import { PaletteDark } from "./styles/Palette";
import { IsMobileApp } from "./utils/MobileUtils";

export const InitializeMobile = async (): Promise<void> => {
  if (!IsMobileApp()) {
    return;
  }

  await PreventStatusBarOverlay();
  InterceptMobileBackButton();
};

const PreventStatusBarOverlay = async (): Promise<void> => {
  // prevent overlay (content will layout below the status bar)
  await StatusBar.setOverlaysWebView({ overlay: false });
  await StatusBar.setBackgroundColor({
    color: PaletteDark.background?.paper ?? "#1E1E1E",
  });
  await StatusBar.setStyle({ style: Style.Dark });
};

const InterceptMobileBackButton = (): void => {
  App.addListener("backButton", ({ canGoBack }) => {
    if (canGoBack) {
      window.history.back();
    } else {
      App.exitApp();
    }
  });
};
