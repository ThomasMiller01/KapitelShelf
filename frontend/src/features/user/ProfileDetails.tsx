import { Box, Container, Stack, TextField, Tooltip } from "@mui/material";
import type { ReactElement } from "react";

import { useUserProfile } from "../../contexts/UserProfileContext";
import { ProfileImageTypeDTO } from "../../lib/api/KapitelShelf.Api/api";
import {
  ProfileImageTypeToName,
  ProfileImageTypeToSrc,
} from "../../utils/UserProfile";

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
            bgcolor: profile?.color,
            pb: "10px",
            borderRadius: "32px",
          }}
        >
          <Tooltip
            title={
              ProfileImageTypeToName[
                profile?.image ?? ProfileImageTypeDTO.NUMBER_0
              ]
            }
          >
            <img
              style={{
                minHeight: "170px",
                maxHeight: "200px",
              }}
              src={
                ProfileImageTypeToSrc[
                  profile?.image ?? ProfileImageTypeDTO.NUMBER_0
                ]
              }
              alt={
                ProfileImageTypeToName[
                  profile?.image ?? ProfileImageTypeDTO.NUMBER_0
                ]
              }
            />
          </Tooltip>
        </Box>
        <Stack spacing={2}>
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
