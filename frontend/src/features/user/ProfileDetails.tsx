import { Box, Container, Stack, TextField } from "@mui/material";
import type { ReactElement } from "react";

import WizardProfile from "../../assets/Wizard.png";
import { useUserProfile } from "../../contexts/UserProfileContext";
import { GetUserColor } from "../../utils/UserProfile";

export const ProfileDetails = (): ReactElement => {
  const { profile } = useUserProfile();

  return (
    <Container maxWidth="sm">
      <Stack
        direction={{ xs: "column", sm: "row" }}
        spacing={{ xs: 2, sm: 4 }}
        alignItems="center"
      >
        <Box
          sx={{
            bgcolor: GetUserColor(profile?.username),
            pb: "10px",
            borderRadius: "32px",
          }}
        >
          <img
            style={{
              minHeight: "170px",
              maxHeight: "200px",
            }}
            src={WizardProfile}
            alt="User Avatar"
          />
        </Box>
        <TextField
          label="Username"
          variant="filled"
          fullWidth
          value={profile?.username}
          disabled
          sx={{
            input: {
              color: "text.primary",
              "-webkit-text-fill-color": "unset !important",
            },
          }}
        />
      </Stack>
    </Container>
  );
};
