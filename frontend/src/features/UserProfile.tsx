import { Avatar, Box } from "@mui/material";
import type { ReactElement } from "react";

import { useUserProfile } from "../contexts/UserProfileContext";

export const UserProfile = (): ReactElement => {
  const { profile } = useUserProfile();

  return (
    <Box>
      <Avatar
        alt={profile?.username || "Logged Out"}
        src="/avatar.png"
        sx={{ width: 32, height: 32 }}
      />
    </Box>
  );
};
