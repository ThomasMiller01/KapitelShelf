import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import PeopleIcon from "@mui/icons-material/People";
import { Box, Divider } from "@mui/material";
import { type ReactElement, useState } from "react";
import { Link } from "react-router-dom";

import DeleteDialog from "../../components/base/feedback/DeleteDialog";
import { IconButtonWithTooltip } from "../../components/base/IconButtonWithTooltip";
import ItemAppBar from "../../components/base/ItemAppBar";
import { ProfileDetails } from "../../features/user/ProfileDetails";
import { useUserProfile } from "../../hooks/useUserProfile";
import { useDeleteUser } from "../../lib/requests/users/useDeleteUser";

export const ViewProfilePage = (): ReactElement => {
  const { clearProfile } = useUserProfile();

  const { mutateAsync: deleteProfile } = useDeleteUser();

  const [deleteOpen, setDeleteOpen] = useState(false);
  const onDelete = async (): Promise<void> => {
    setDeleteOpen(false);

    await deleteProfile();
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
