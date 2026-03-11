import { Device } from "@capacitor/device";
import { useEffect, useState } from "react";

import { IsMobileApp } from "../../../../utils/MobileUtils";

interface NavigatorWithBattery extends Navigator {
  getBattery?: () => Promise<{
    level: number;
    charging: boolean;
  }>;
}

interface ReaderBatteryResult {
  batteryPercent: number | null;
  isCharging?: boolean;
}

const BATTERY_REFRESH_INTERVAL_MS = 45000;

export const useReaderBattery = (): ReaderBatteryResult => {
  const [batteryPercent, setBatteryPercent] = useState<number | null>(null);
  const [isCharging, setIsCharging] = useState<boolean | undefined>(undefined);

  useEffect(() => {
    let cancelled = false;

    const setBattery = (
      level: number | null | undefined,
      charging?: boolean,
    ): void => {
      if (cancelled) {
        return;
      }

      if (typeof level === "number" && level >= 0) {
        setBatteryPercent(Math.round(level * 100));
      } else {
        setBatteryPercent(null);
      }

      setIsCharging(typeof charging === "boolean" ? charging : undefined);
    };

    const readBattery = async (): Promise<void> => {
      try {
        if (IsMobileApp()) {
          const batteryInfo = await Device.getBatteryInfo();
          setBattery(batteryInfo.batteryLevel, batteryInfo.isCharging);
          return;
        }

        const batteryNavigator = navigator as NavigatorWithBattery;
        if (typeof batteryNavigator.getBattery === "function") {
          const battery = await batteryNavigator.getBattery();
          setBattery(battery.level, battery.charging);
          return;
        }
      } catch {
        // Unsupported/failed battery APIs should silently hide battery info.
      }

      setBattery(null, undefined);
    };

    readBattery();
    const interval = window.setInterval(() => readBattery(), BATTERY_REFRESH_INTERVAL_MS);

    return () => {
      cancelled = true;
      window.clearInterval(interval);
    };
  }, []);

  return { batteryPercent, isCharging };
};
