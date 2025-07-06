import { useMutation } from "@tanstack/react-query";
import type { ReactElement, ReactNode } from "react";
import { createContext, useContext, useEffect, useState } from "react";

import { usersApi } from "../lib/api/KapitelShelf.Api";
import type { UserDTO } from "../lib/api/KapitelShelf.Api/api";

interface UserProfileContextProps {
  profile: UserDTO | null;
  setProfile: (profile: UserDTO) => void;
  clearProfile: () => void;
  syncProfile: () => void;
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

  const { mutateAsync: mutateLoadProfile } = useMutation({
    mutationKey: ["load-profile"],
    mutationFn: async (userId: string) => {
      const { data } = await usersApi.usersUserIdGet(userId);
      return data;
    },
  });

  // load profile on mount
  useEffect(() => {
    const stored = localStorage.getItem(`kapitelshelf.${PROFILE_KEY}`);
    if (stored) {
      const storedProfile = JSON.parse(stored);
      setProfileState(storedProfile);

      // update user profile from api
      mutateLoadProfile(storedProfile.id).then((fetchedProfile) => {
        setProfileState(fetchedProfile);
      });
    }
  }, [mutateLoadProfile]);

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

  const syncProfile = (): void => {
    if (!profile || !profile.id) {
      return;
    }

    mutateLoadProfile(profile.id).then((fetchedProfile) => {
      setProfileState(fetchedProfile);
    });
  };

  return (
    <UserProfileContext.Provider
      value={{ profile, setProfile, clearProfile, syncProfile }}
    >
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
