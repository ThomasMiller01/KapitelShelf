import {
  createContext,
  type ReactElement,
  type ReactNode,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from "react";

import { useUserProfile } from "../hooks/useUserProfile";
import { usersApi } from "../lib/api/KapitelShelf.Api";
import type { UserSettingDTO } from "../lib/api/KapitelShelf.Api/api";
import type { UserSettingValueType } from "../utils/UserProfileUtils";
import {
  UserSettingValueFromDto,
  UserSettingValueToDto,
} from "../utils/UserProfileUtils";

interface ContextValue {
  getSetting: (key: string) => UserSettingValueType | null;
  setSetting: (key: string, value: UserSettingValueType) => void;
  isLoaded: boolean;
}

export const GET_KEY = (userId: string): string =>
  `kapitelshelf.${userId}.settings`;

export const UserSettingsContext = createContext<ContextValue | undefined>(
  undefined
);

export const UserSettingsProvider = ({
  children,
}: {
  children: ReactNode;
}): ReactElement => {
  const { profile } = useUserProfile();
  const [settings, setSettings] = useState<UserSettingDTO[]>([]);
  const [isLoaded, setIsLoaded] = useState(false);

  useEffect(() => {
    const load = async (): Promise<void> => {
      if (!profile?.id) {
        return;
      }

      setSettings([]);
      setIsLoaded(false);

      try {
        const res = await usersApi.usersUserIdSettingsGet(profile.id);
        setSettings(res.data ?? []);
      } catch {
        // fallback to localstorage if cloud request fails
        const raw = localStorage.getItem(GET_KEY(profile.id));
        if (raw) {
          setSettings(JSON.parse(raw));
        }
      } finally {
        setIsLoaded(true); // only mark loaded once cloud/local is settled
      }
    };

    void load();
  }, [profile?.id]);

  useEffect(() => {
    if (!profile?.id || !isLoaded) {
      return;
    }

    localStorage.setItem(GET_KEY(profile.id), JSON.stringify(settings));
  }, [settings, isLoaded, profile?.id]);

  const getSetting = useCallback(
    (key: string): UserSettingValueType | null => {
      const s = settings.find((x) => x.key === key);
      if (!s) {
        return null;
      }

      return UserSettingValueFromDto(s);
    },
    [settings]
  );

  const setSetting = useCallback(
    (key: string, value: UserSettingValueType): void => {
      if (!profile?.id) {
        return;
      }

      const dto: UserSettingDTO = UserSettingValueToDto(key, value);

      setSettings((prev) => {
        const filtered = prev.filter((s) => s.key !== key);
        return [...filtered, dto];
      });

      // ignore errors
      void usersApi.usersUserIdSettingsPut(profile.id, dto);
    },
    [profile?.id]
  );

  const value = useMemo<ContextValue>(
    () => ({ getSetting, setSetting, isLoaded }),
    [getSetting, setSetting, isLoaded]
  );

  return (
    <UserSettingsContext.Provider value={value}>
      {children}
    </UserSettingsContext.Provider>
  );
};
