import { useCallback } from "react";

import { useUserProfile } from "../contexts/UserProfileContext";

const BASE_LOCAL_STORAGE_KEY = "kapitelshelf";

export const useLocalStorage = (): [
  (key: string) => string | null,
  (key: string, value: string) => void,
  (key: string) => string
] => {
  const { profile } = useUserProfile();

  const getKey = useCallback(
    (key: string): string => `${BASE_LOCAL_STORAGE_KEY}.${profile?.id}.${key}`,
    [profile?.id]
  );

  const getItem = useCallback(
    (key: string): string | null => localStorage.getItem(getKey(key)),
    [getKey]
  );

  const setItem = useCallback(
    (key: string, value: string): void =>
      localStorage.setItem(getKey(key), value),
    [getKey]
  );

  return [getItem, setItem, getKey];
};
