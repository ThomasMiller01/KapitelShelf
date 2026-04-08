import { App } from "@capacitor/app";
import { StatusBar, Style } from "@capacitor/status-bar";

import { restoreAppOrientation } from "./features/reader/hooks/device/readerOrientation";
import { IsMobileApp } from "./shared/utils/MobileUtils";

export type MobileColorMode = "light" | "dark";

export const InitializeMobile = async (): Promise<void> => {
  if (!IsMobileApp()) {
    return;
  }

  InterceptMobileBackButton();
  await restoreAppOrientation();
};

export const ApplyModeToMobile = async (mode: MobileColorMode): Promise<void> => {
  if (!IsMobileApp()) {
    return;
  }

  await StatusBar.setStyle({
    style: mode === "dark" ? Style.Dark : Style.Light,
  });
  await StatusBar.setBackgroundColor({
    color: mode === "dark" ? "#000000" : "#ffffff",
  });
};

export const HideMobileStatusBar = async (): Promise<void> => {
  if (!IsMobileApp()) {
    return;
  }

  await StatusBar.hide();
};

export const ShowMobileStatusBar = async (): Promise<void> => {
  if (!IsMobileApp()) {
    return;
  }

  await StatusBar.show();
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
