import PeopleIcon from "@mui/icons-material/People";
import {
  Avatar,
  Divider,
  IconButton,
  ListItemIcon,
  Menu,
  MenuItem,
} from "@mui/material";
import type { ReactElement } from "react";
import React from "react";
import { Link } from "react-router-dom";

import FancyText from "../../components/FancyText";
import { TasksMenuItem } from "../../components/tasks/TasksMenuItem";
import { useUserProfile } from "../../contexts/UserProfileContext";
import {
  ProfileImageTypeDTO,
  type UserDTO,
} from "../../lib/api/KapitelShelf.Api/api";
import { ProfileImageTypeToSrc } from "../../utils/UserProfileUtils";

export const ProfileMenu = (): ReactElement => {
  const { profile, clearProfile } = useUserProfile();

  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClick = (event: React.MouseEvent<HTMLElement>): void => {
    setAnchorEl(event.currentTarget);
  };
  const handleClose = (): void => {
    setAnchorEl(null);
  };
  const handleSwitchProfile = (): void => {
    handleClose();
    clearProfile();
  };

  return (
    <React.Fragment>
      <IconButton onClick={handleClick} size="small">
        <UserProfileIcon profile={profile} />
      </IconButton>
      <Menu
        anchorEl={anchorEl}
        open={open}
        disableScrollLock
        onClose={handleClose}
        onClick={handleClose}
        slotProps={{
          paper: {
            elevation: 0,
            sx: {
              overflow: "visible",
              filter: "drop-shadow(0px 2px 8px rgba(0,0,0,0.32))",
              mt: 1.5,
              "& .MuiAvatar-root": {
                width: 32,
                height: 32,
                ml: -0.5,
                mr: 1,
              },
              "&::before": {
                content: '""',
                display: "block",
                position: "absolute",
                top: 0,
                right: 14,
                width: 10,
                height: 10,
                bgcolor: "background.paper",
                transform: "translateY(-50%) rotate(45deg)",
                zIndex: 0,
              },
            },
          },
        }}
        transformOrigin={{ horizontal: "right", vertical: "top" }}
        anchorOrigin={{ horizontal: "right", vertical: "bottom" }}
      >
        <MenuItem component={Link} to="/profile">
          <UserProfileIcon profile={profile} />
          <FancyText ml="6px">{profile?.username}</FancyText>
        </MenuItem>
        <Divider />
        <TasksMenuItem />
        <Divider />
        <MenuItem onClick={handleSwitchProfile}>
          <ListItemIcon>
            <PeopleIcon fontSize="small" />
          </ListItemIcon>
          Switch Profile
        </MenuItem>
      </Menu>
    </React.Fragment>
  );
};

interface UserProfileIconProps {
  profile: UserDTO | null;
}

const UserProfileIcon = ({ profile }: UserProfileIconProps): ReactElement => (
  <Avatar
    alt={profile?.username || "Unknown User"}
    src={ProfileImageTypeToSrc[profile?.image ?? ProfileImageTypeDTO.NUMBER_0]}
    variant="rounded"
    sx={{
      width: 32,
      height: 32,
      bgcolor: profile?.color,
    }}
  />
);
