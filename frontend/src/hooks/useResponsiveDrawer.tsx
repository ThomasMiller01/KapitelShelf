import { useCallback, useState } from "react";

import { useLocalStorage } from "./useLocalStorage";
import { useMobile } from "./useMobile";

const OPEN_KEY = "sidebar.open";

export const useResponsiveDrawer = (): [boolean, () => void] => {
  const { isMobile } = useMobile();
  const [getItem, setItem] = useLocalStorage();

  const getInitialState = useCallback((): boolean => {
    if (isMobile) {
      // mobile defaults to closed
      return false;
    }

    const stored = getItem(OPEN_KEY);
    if (stored === null) {
      // desktop defaults to open
      return true;
    }

    return stored === "true";
  }, [getItem, isMobile]);

  const [open, setOpen] = useState(getInitialState);

  const toggleDrawer = (): void => {
    const newValue = !open;
    setOpen(newValue);

    if (!isMobile) {
      // store new value only on desktop
      setItem(OPEN_KEY, String(newValue));
    }
  };

  return [open, toggleDrawer];
};
