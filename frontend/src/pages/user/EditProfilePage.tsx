import CloseIcon from "@mui/icons-material/Close";
import { Box, Button, Chip, Container, styled } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { type ReactElement, useState } from "react";
import { Link, useNavigate } from "react-router-dom";

import DeleteDialog from "../../components/base/feedback/DeleteDialog";
import ItemAppBar from "../../components/base/ItemAppBar";
import { useUserProfile } from "../../contexts/UserProfileContext";
import EditableProfileDetails from "../../features/user/EditableProfileDetails";
import { useMobile } from "../../hooks/useMobile";
import { usersApi } from "../../lib/api/KapitelShelf.Api";
import type { UserDTO } from "../../lib/api/KapitelShelf.Api/api";

const EditingBadge = styled(Chip, {
  shouldForwardProp: (prop) => prop !== "isMobile",
})<{ isMobile: boolean }>(({ isMobile }) => ({
  fontSize: isMobile ? "0.82rem" : "0.95rem",
}));

export const EditProfilePage = (): ReactElement => {
  const navigate = useNavigate();
  const { isMobile } = useMobile();
  const { profile, clearProfile, syncProfile } = useUserProfile();

  const { mutateAsync: mutateDeleteProfile } = useMutation({
    mutationKey: ["delete-profile", profile?.id],
    mutationFn: async () => {
      if (profile?.id === undefined) {
        return null;
      }

      await usersApi.usersUserIdDelete(profile.id);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Deleting profile",
        showLoading: true,
        showSuccess: true,
      },
    },
  });

  const { mutateAsync: mutateEditProfile } = useMutation({
    mutationKey: ["edit-profile", profile?.id],
    mutationFn: async (user: UserDTO) => {
      if (profile?.id === undefined) {
        return null;
      }

      await usersApi.usersUserIdPut(profile.id, user);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Updating profile",
        showLoading: true,
        showSuccess: true,
      },
    },
  });

  const [deleteOpen, setDeleteOpen] = useState(false);
  const onDelete = async (): Promise<void> => {
    setDeleteOpen(false);

    await mutateDeleteProfile();
    clearProfile();
  };

  const onConfirm = (user: UserDTO): void => {
    mutateEditProfile(user).then(() => {
      syncProfile();
      navigate(-1);
    });
  };

  return (
    <Box>
      <ItemAppBar
        title="Your Profile"
        addons={[
          <EditingBadge key="editing" label="EDIT" isMobile={isMobile} />,
        ]}
        actions={[
          <Button
            component={Link}
            to="/profile"
            key="cancel"
            startIcon={<CloseIcon />}
            variant="contained"
            size="small"
          >
            Cancel
          </Button>,
        ]}
      />
      <Container maxWidth="sm" sx={{ my: "24px" }}>
        <EditableProfileDetails
          initial={profile}
          confirmAction={{
            name: "Edit Profile",
            onClick: onConfirm,
          }}
        />
      </Container>
      <DeleteDialog
        open={deleteOpen}
        onCancel={() => setDeleteOpen(false)}
        onConfirm={onDelete}
        title="Confirm to delete this profile"
        description="Are you sure you want to delete this profile? This action cannot be undone and all data is lost."
      />
    </Box>
  );
};
