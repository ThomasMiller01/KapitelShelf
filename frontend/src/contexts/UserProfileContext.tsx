import type { ReactElement, ReactNode } from "react";
import { createContext, useContext, useEffect, useState } from "react";

import type { UserDTO } from "../lib/api/KapitelShelf.Api/api";

interface UserProfileContextProps {
  profile: UserDTO | null;
  setProfile: (profile: UserDTO) => void;
  clearProfile: () => void;
}

const PROFILE_KEY = "current.user.profile";

const UserProfileContext = createContext<UserProfileContextProps | undefined>(
  undefined
);

export const UserProfileProvider = ({
  children,
}: {
  children: ReactNode;
}): ReactElement => {
  const [profile, setProfileState] = useState<UserDTO | null>(null);

  // load profile on mount
  useEffect(() => {
    const stored = localStorage.getItem(`kapitelshelf.${PROFILE_KEY}`);
    if (stored) {
      setProfileState(JSON.parse(stored));
    }
  }, []);

  // save when profile changes
  useEffect(() => {
    if (profile) {
      localStorage.setItem(
        `kapitelshelf.${PROFILE_KEY}`,
        JSON.stringify(profile)
      );
    }
  }, [profile]);

  const setProfile = (newProfile: UserDTO): void => {
    setProfileState(newProfile);
  };

  const clearProfile = (): void => {
    setProfileState(null);
    localStorage.removeItem(`kapitelshelf.${PROFILE_KEY}`);
  };

  return (
    <UserProfileContext.Provider value={{ profile, setProfile, clearProfile }}>
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
