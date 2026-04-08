import CloseIcon from "@mui/icons-material/Close";
import { Box, Button, Chip, Container, styled } from "@mui/material";
import { type ReactElement, useState } from "react";
import { Link, useNavigate } from "react-router-dom";

import {
  EditableProfileDetails,
  useDeleteUser,
  useEditUser,
} from "../../features/user";
import ConfirmDialog from "../../shared/components/base/feedback/ConfirmDialog";
import ItemAppBar from "../../shared/components/base/ItemAppBar";
import { useMobile } from "../../shared/hooks/useMobile";
import { useUserProfile } from "../../shared/hooks/useUserProfile";
import type { UserDTO } from "../../lib/api/KapitelShelf.Api/api";

const EditingBadge = styled(Chip, {
  shouldForwardProp: (prop) => prop !== "isMobile",
})<{ isMobile: boolean }>(({ isMobile }) => ({
  fontSize: isMobile ? "0.82rem" : "0.95rem",
}));

export const EditProfilePage = (): ReactElement => {
  const { isMobile } = useMobile();
  const navigate = useNavigate();

  const { profile, clearProfile, syncProfile } = useUserProfile();

  const { mutateAsync: deleteProfile } = useDeleteUser();
  const { mutateAsync: editProfile } = useEditUser();

  const [deleteOpen, setDeleteOpen] = useState(false);
  const onDelete = async (): Promise<void> => {
    setDeleteOpen(false);

    await deleteProfile();
    clearProfile();
  };

  const onConfirm = (user: UserDTO | undefined): void => {
    if (user === undefined) {
      return;
    }

    editProfile(user).then(() => {
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
      <ConfirmDialog
        open={deleteOpen}
        onCancel={() => setDeleteOpen(false)}
        onConfirm={onDelete}
        title="Confirm to delete this profile"
        description="Are you sure you want to delete this profile? This action cannot be undone and all data is lost."
      />
    </Box>
  );
};
