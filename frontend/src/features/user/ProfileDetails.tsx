import { Container, Stack, TextField } from "@mui/material";
import type { ReactElement } from "react";

import { ProfileImage } from "../../components/ProfileImage";
import { useMobile } from "../../hooks/useMobile";
import { useUserProfile } from "../../hooks/useUserProfile";
import { ProfileImageTypeDTO } from "../../lib/api/KapitelShelf.Api/api";

export const ProfileDetails = (): ReactElement => {
  const { isMobile } = useMobile();
  const { profile } = useUserProfile();

  return (
    <Container maxWidth="sm">
      <Stack
        direction={{ xs: "column", sm: "row" }}
        spacing={{ xs: 2, sm: 4 }}
        alignItems="center"
      >
        <ProfileImage
          profileImageType={profile?.image ?? ProfileImageTypeDTO.NUMBER_0}
          profileColor={profile?.color ?? ""}
        />
        <Stack spacing={2} sx={{ width: isMobile ? "80%" : "100%" }}>
          <TextField
            label="Username"
            variant="filled"
            fullWidth
            value={profile?.username}
            disabled
            sx={{
              input: {
                color: "text.primary",
                WebkitTextFillColor: "unset !important",
              },
            }}
          />
        </Stack>
      </Stack>
    </Container>
  );
};
