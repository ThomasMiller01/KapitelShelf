import type { Dispatch, SetStateAction } from "react";
import { useEffect, useState } from "react";

import { useLocalStorage } from "./useLocalStorage";

export const useSetting = <T,>(
  key: string,
  defaultValue: T
): [T, Dispatch<SetStateAction<T>>] => {
  const [getItem, setItem] = useLocalStorage();

  const getInitialValue = (): T => {
    try {
      const stored = getItem(key);
      if (stored === null) {
        return defaultValue;
      }

      return JSON.parse(stored) as T;
    } catch {
      return defaultValue;
    }
  };

  const [value, setValue] = useState<T>(getInitialValue);

  useEffect(() => {
    try {
      setItem(key, JSON.stringify(value));
    } catch {
      // ignore error
    }
  }, [key, value, setItem]);

  return [value, setValue];
};
