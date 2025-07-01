import type { ReactElement, ReactNode } from "react";
import { createContext, useContext, useEffect, useState } from "react";

import { useLocalStorage } from "../hooks/useLocalStorage";
import type { UserDTO } from "../lib/api/KapitelShelf.Api/api";

interface UserProfileContextProps {
  profile: UserDTO | null;
  setProfile: (profile: UserDTO) => void;
}

const PROFILE_KEY = "user.profile";

const UserProfileContext = createContext<UserProfileContextProps | undefined>(
  undefined
);

export const UserProfileProvider = ({
  children,
}: {
  children: ReactNode;
}): ReactElement => {
  const [getItem, setItem] = useLocalStorage();
  const [profile, setProfileState] = useState<UserDTO | null>(null);

  // load profile on mount
  useEffect(() => {
    const stored = getItem(PROFILE_KEY);
    if (stored) {
      setProfileState(JSON.parse(stored));
    }
  }, [getItem]);

  // save when profile changes
  useEffect(() => {
    if (profile) {
      setItem(PROFILE_KEY, JSON.stringify(profile));
    }
  }, [profile, setItem]);

  const setProfile = (newProfile: UserDTO): void => {
    setProfileState(newProfile);
  };

  return (
    <UserProfileContext.Provider value={{ profile, setProfile }}>
      {children}
    </UserProfileContext.Provider>
  );
};

// Custom hook for using the profile context
export const useUserProfile = (): UserProfileContextProps => {
  const ctx = useContext(UserProfileContext);
  if (!ctx) {
    throw new Error(
      "useUserProfile() must be used within a UserProfileProvider"
    );
  }
  return ctx;
};
