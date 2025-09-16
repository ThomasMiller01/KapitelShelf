import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import PeopleIcon from "@mui/icons-material/People";
import { Box, Divider } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import { type ReactElement, useState } from "react";
import { Link } from "react-router-dom";

import DeleteDialog from "../../components/base/feedback/DeleteDialog";
import { IconButtonWithTooltip } from "../../components/base/IconButtonWithTooltip";
import ItemAppBar from "../../components/base/ItemAppBar";
import { ProfileDetails } from "../../features/user/ProfileDetails";
import { useUserProfile } from "../../hooks/useUserProfile";
import { usersApi } from "../../lib/api/KapitelShelf.Api";

export const ViewProfilePage = (): ReactElement => {
  const { profile, clearProfile } = useUserProfile();

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

  const [deleteOpen, setDeleteOpen] = useState(false);
  const onDelete = async (): Promise<void> => {
    setDeleteOpen(false);

    await mutateDeleteProfile();
    clearProfile();
  };

  return (
    <Box>
      <ItemAppBar
        title="Your Profile"
        actions={[
          <IconButtonWithTooltip
            onClick={clearProfile}
            key="switch"
            tooltip="Switch Profile"
          >
            <PeopleIcon />
          </IconButtonWithTooltip>,
          <Divider orientation="vertical" flexItem key="divider" />,
          <IconButtonWithTooltip
            component={Link}
            to="/profile/edit"
            key="edit"
            tooltip="Edit Profile"
          >
            <EditIcon />
          </IconButtonWithTooltip>,
          <IconButtonWithTooltip
            onClick={() => setDeleteOpen(true)}
            key="delete"
            tooltip="Delete Profile"
          >
            <DeleteIcon />
          </IconButtonWithTooltip>,
        ]}
      />
      <Box padding="24px">
        <ProfileDetails />
      </Box>
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
