import { useCallback, useState } from "react";

import { useLocalStorage } from "./useLocalStorage";

const SHOWN_KEY = "welcome-screen.open";

export const useWelcomeScreen = (): [boolean] => {
  const [getItem, setItem] = useLocalStorage();

  const getInitialState = useCallback((): boolean => {
    const stored = getItem(SHOWN_KEY);

    // hide on next visit
    setItem(SHOWN_KEY, "true");

    if (stored === null) {
      // show welcome screen once
      return false;
    }

    return stored === "true";
  }, [getItem, setItem]);

  const [isShown, _] = useState(getInitialState);

  return [isShown];
};
