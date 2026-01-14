import { Box, Container, Divider, Typography } from "@mui/material";
import { type ReactElement } from "react";
import { useNavigate } from "react-router-dom";

import EditableProfileDetails from "../../features/user/EditableProfileDetails";
import { useUserProfile } from "../../hooks/useUserProfile";
import type { UserDTO } from "../../lib/api/KapitelShelf.Api/api";
import { useCreateUser } from "../../lib/requests/users/useCreateUser";

export const CreateProfilePage = (): ReactElement => {
  const navigate = useNavigate();
  const { setProfile } = useUserProfile();

  const { mutateAsync: createUserProfile } = useCreateUser();

  const onSave = (user: UserDTO | undefined): void => {
    if (user === undefined) {
      return;
    }

    createUserProfile(user).then((response) => {
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
