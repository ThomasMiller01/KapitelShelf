import { useContext } from "react";

import type { UserProfileContextProps } from "../contexts/UserProfileContext";
import { UserProfileContext } from "../contexts/UserProfileContext";

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
