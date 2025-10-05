import type { Dispatch, SetStateAction } from "react";
import { useCallback, useContext, useEffect, useRef, useState } from "react";

import { GET_KEY, UserSettingsContext } from "../contexts/UserSettingsContext";
import type { UserSettingDTO } from "../lib/api/KapitelShelf.Api/api";
import type { UserSettingValueType } from "../utils/UserProfileUtils";
import {
  UserSettingValueCoerceToType,
  UserSettingValueFromDto,
} from "../utils/UserProfileUtils";
import { useUserProfile } from "./useUserProfile";

export const useSetting = <T extends UserSettingValueType>(
  key: string,
  defaultValue: T
): [T, Dispatch<SetStateAction<T>>] => {
  const ctx = useContext(UserSettingsContext);
  if (!ctx) {
    throw new Error("useSetting must be used inside UserSettingsProvider");
  }

  // freeze the initial default to avoid effect churn when caller passes new literals
  const defaultRef = useRef<T>(defaultValue);

  const { profile } = useUserProfile();
  const { getSetting, setSetting, isLoaded } = ctx;

  // helper: load from localStorage manually
  const loadFromLocalStorage = useCallback((): T => {
    if (!profile?.id) {
      return defaultRef.current;
    }

    try {
      const raw = localStorage.getItem(GET_KEY(profile.id));
      if (!raw) {
        return defaultRef.current;
      }

      const parsed = JSON.parse(raw) as UserSettingDTO[];
      const found = parsed.find((s) => s.key === key);
      if (!found) {
        return defaultRef.current;
      }

      return UserSettingValueCoerceToType<T>(
        UserSettingValueFromDto(found),
        defaultRef.current
      );
    } catch {
      return defaultRef.current;
    }
  }, [key, profile?.id]);

  const [value, setValue] = useState<T>(loadFromLocalStorage());
  const lastProfileId = useRef<string | null>(null);

  // reset on profile switch
  useEffect(() => {
    if (!profile?.id) {
      return;
    }

    if (profile.id !== lastProfileId.current) {
      lastProfileId.current = profile.id;

      const local = loadFromLocalStorage();
      setValue(local);
    }
  }, [profile?.id, key, defaultRef, loadFromLocalStorage]);

  // hydrate from cloud when ready
  useEffect(() => {
    if (!isLoaded) {
      return;
    }

    const cloud = getSetting(key);
    if (cloud === null) {
      setSetting(key, value); // persist current value (was from local)
    } else {
      const coerced = UserSettingValueCoerceToType<T>(
        cloud,
        defaultRef.current
      );
      setValue((prev) => (Object.is(prev, coerced) ? prev : coerced));
    }
  }, [isLoaded, key]);

  // persist user changes
  useEffect(() => {
    if (!isLoaded) {
      return;
    }

    const cloudVal = getSetting(key);
    const coerced =
      cloudVal !== null
        ? UserSettingValueCoerceToType<T>(cloudVal, defaultRef.current)
        : null;

    if (coerced === null || !Object.is(coerced, value)) {
      setSetting(key, value);
    }
  }, [value]);

  return [value, setValue];
};
