import { App } from "@capacitor/app";
import { StatusBar, Style } from "@capacitor/status-bar";

import { IsMobileApp } from "./utils/MobileUtils";

export const InitializeMobile = async (): Promise<void> => {
  if (!IsMobileApp()) {
    return;
  }

  InterceptMobileBackButton();
};

export const ApplyModeToMobile = async (
  mode: "light" | "dark"
): Promise<void> => {
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

const InterceptMobileBackButton = (): void => {
  App.addListener("backButton", ({ canGoBack }) => {
    if (canGoBack) {
      window.history.back();
    } else {
      App.exitApp();
    }
  });
};
