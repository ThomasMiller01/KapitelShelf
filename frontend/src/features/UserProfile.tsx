import { Avatar, Box } from "@mui/material";
import type { ReactElement } from "react";

import TestImg from "../assets/test.png";
import { useUserProfile } from "../contexts/UserProfileContext";
import { GetUserColor } from "../utils/UserProfile";

export const UserProfile = (): ReactElement => {
  const { profile } = useUserProfile();

  return (
    <Box>
      <Avatar
        alt={profile?.username || "Unknown User"}
        src={TestImg}
        variant="rounded"
        sx={{
          width: 32,
          height: 32,
          bgcolor: GetUserColor(profile?.username),
        }}
      />
    </Box>
  );
};
