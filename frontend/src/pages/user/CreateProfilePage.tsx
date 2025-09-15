import { Box, Container, Divider, Typography } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { type ReactElement } from "react";
import { useNavigate } from "react-router-dom";

import EditableProfileDetails from "../../features/user/EditableProfileDetails";
import { useUserProfile } from "../../hooks/useUserProfile";
import { usersApi } from "../../lib/api/KapitelShelf.Api";
import type {
  CreateUserDTO,
  UserDTO,
} from "../../lib/api/KapitelShelf.Api/api";

export const CreateProfilePage = (): ReactElement => {
  const navigate = useNavigate();
  const { setProfile } = useUserProfile();

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

  const onSave = (user: UserDTO | undefined): void => {
    if (user === undefined) {
      return;
    }

    mutateCreateUserProfile(user).then((response) => {
      setProfile(response.data);
      navigate(-1);
    });
  };

  const onCancel = (): void => {
    navigate(-1);
  };

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
        <Divider sx={{ mb: 2.5, mt: 2 }} />
        <EditableProfileDetails
          confirmAction={{
            name: "Save",
            onClick: onSave,
          }}
          cancelAction={{
            name: "Cancel",
            onClick: onCancel,
          }}
        />
      </Container>
    </Box>
  );
};
