import { useEffect } from "react";

import {
  HideMobileStatusBar,
  ShowMobileStatusBar,
} from "../../../../InitializeMobile";

const STATUS_BAR_REHIDE_INTERVAL_MS = 1000;

export const useReaderStatusBar = (): void => {
  useEffect(() => {
    void HideMobileStatusBar();

    const interval = window.setInterval(() => {
      void HideMobileStatusBar();
    }, STATUS_BAR_REHIDE_INTERVAL_MS);

    return () => {
      window.clearInterval(interval);
      void ShowMobileStatusBar();
    };
  }, []);
};
