import { useEffect, useState } from "react";

import { useMobile } from "./useMobile";
import { useSetting } from "./useSetting";

const OPEN_KEY = "sidebar.open";

export const useResponsiveDrawer = (): [boolean, () => void] => {
  const { isMobile } = useMobile();
  const [setting, setSetting] = useSetting<boolean>(OPEN_KEY, true);

  const [localOpen, setLocalOpen] = useState(false);

  // sync setting to localOpen when switching to mobile
  useEffect(() => {
    if (isMobile) {
      setLocalOpen(false);
    }
  }, [isMobile]);

  const open = isMobile ? localOpen : setting;

  const toggleDrawer = (): void => {
    if (isMobile) {
      setLocalOpen((prev) => !prev);
    } else {
      setSetting(!setting);
    }
  };

  return [open, toggleDrawer];
};
