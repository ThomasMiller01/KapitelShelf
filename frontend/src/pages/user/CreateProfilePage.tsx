import {
  Box,
  Button,
  Container,
  Divider,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { type ReactElement, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import WizardProfile from "../../assets/Wizard.png";
import { useUserProfile } from "../../contexts/UserProfileContext";
import { usersApi } from "../../lib/api/KapitelShelf.Api";
import type { CreateUserDTO } from "../../lib/api/KapitelShelf.Api/api";
import { GetUserColor } from "../../utils/UserProfile";

// 600ms after user stops typing
const USERNAME_REST_MS = 600;

export const CreateProfilePage = (): ReactElement => {
  const navigate = useNavigate();
  const { setProfile } = useUserProfile();

  const [username, setUsername] = useState("");
  const [profileImageColor, setProfileImageColor] = useState(
    GetUserColor(username)
  );

  const { mutateAsync: mutateCreateUserProfile } = useMutation({
    mutationKey: ["create-user-profile"],
    mutationFn: async (createUser: CreateUserDTO) =>
      usersApi.usersPost(createUser),
    meta: {
      notify: {
        enabled: true,
        operation: "Adding profile",
        showLoading: true,
        showSuccess: true,
      },
    },
  });

  const onSave = (): void => {
    mutateCreateUserProfile({ username }).then((user) => {
      setProfile(user.data);
      navigate(-1);
    });
  };

  const onCancel = (): void => {
    navigate(-1);
  };

  // update profile image color with delay
  useEffect(() => {
    const handle = setTimeout(
      () => setProfileImageColor(GetUserColor(username)),
      USERNAME_REST_MS
    );
    return (): void => clearTimeout(handle);
  }, [username]);

  return (
    <Box
      minHeight="90vh"
      display="flex"
      padding="20px"
      flexDirection="column"
      alignItems="center"
      justifyContent="center"
      bgcolor="background.default"
    >
      <Container maxWidth="sm">
        <Typography variant="h4" gutterBottom>
          Add Profile
        </Typography>
        <Typography variant="body1" gutterBottom>
          Add a profile for another person that is using KapitelShelf.
        </Typography>
        <Divider sx={{ mb: 4, mt: 2 }} />
        <Stack
          direction={{ xs: "column-reverse", sm: "row" }}
          spacing={{ xs: 2, sm: 4 }}
          alignItems="center"
        >
          <Box
            sx={{
              bgcolor: profileImageColor,
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
              alt={"User Avatar"}
            />
          </Box>
          <TextField
            label="Username"
            variant="filled"
            fullWidth
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            helperText={username === "" ? "Please enter a username." : ""}
            error={username === ""}
          />
        </Stack>
        <Stack direction="row" spacing={2} justifyContent="end" mt={3}>
          <Button
            variant="contained"
            onClick={onSave}
            disabled={username === ""}
          >
            Save
          </Button>
          <Button variant="outlined" onClick={onCancel}>
            Cancel
          </Button>
        </Stack>
      </Container>
    </Box>
  );
};
