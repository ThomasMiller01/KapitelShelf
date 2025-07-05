import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import PeopleIcon from "@mui/icons-material/People";
import { Box, Container, Divider, Stack, TextField } from "@mui/material";
import { type ReactElement } from "react";
import { Link } from "react-router-dom";

import WizardProfile from "../../assets/Wizard.png";
import { IconButtonWithTooltip } from "../../components/base/IconButtonWithTooltip";
import ItemAppBar from "../../components/base/ItemAppBar";
import { useUserProfile } from "../../contexts/UserProfileContext";
import { GetUserColor } from "../../utils/UserProfile";

export const ViewProfilePage = (): ReactElement => {
  const { profile, clearProfile } = useUserProfile();

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
            onClick={() => alert("TODO")}
            key="delete"
            tooltip="Delete Profile"
          >
            <DeleteIcon />
          </IconButtonWithTooltip>,
        ]}
      />
      <Box padding="24px">
        <Container maxWidth="sm">
          <Stack
            direction={{ xs: "column", sm: "row" }}
            spacing={{ xs: 2, sm: 4 }}
            alignItems={{ xs: "center", md: "start" }}
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
                alt={"User Avatar"}
              />
            </Box>
            <TextField
              label="Username"
              variant="filled"
              fullWidth
              value={profile?.username}
              disabled
            />
          </Stack>
        </Container>
      </Box>
    </Box>
  );
};
