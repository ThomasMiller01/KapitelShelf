import DarkModeIcon from "@mui/icons-material/DarkMode";
import LightModeIcon from "@mui/icons-material/LightMode";
import PeopleIcon from "@mui/icons-material/People";
import TrackChangesIcon from "@mui/icons-material/TrackChanges";
import {
  Avatar,
  Divider,
  IconButton,
  ListItemIcon,
  Menu,
  MenuItem,
  useColorScheme,
} from "@mui/material";
import type { ReactElement } from "react";
import React from "react";
import { Link } from "react-router-dom";

import FancyText from "../../components/FancyText";
import { NotificationsBadge } from "../../components/notifications/NotificationsBadge";
import { NotificationsIcon } from "../../components/notifications/NotificationsIcon";
import { TasksMenuItem } from "../../components/tasks/TasksMenuItem";
import { useMobile } from "../../hooks/useMobile";
import { useUserProfile } from "../../hooks/useUserProfile";
import {
  ProfileImageTypeDTO,
  type UserDTO,
} from "../../lib/api/KapitelShelf.Api/api";
import { ProfileImageTypeToSrc } from "../../utils/UserProfileUtils";

export const ProfileMenu = (): ReactElement => {
  const { profile, clearProfile } = useUserProfile();
  const { isMobile } = useMobile();

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

  const { mode, systemMode, setMode } = useColorScheme();

  if (!mode) {
    // mode is undefined on first render
    return <></>;
  }

  const currentMode = mode === "system" ? systemMode : mode;

  return (
    <React.Fragment>
      <NotificationsBadge overlap="circular">
        <IconButton onClick={handleClick} size="small">
          <UserProfileIcon profile={profile} />
        </IconButton>
      </NotificationsBadge>
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
        {/* User Profile */}
        <MenuItem component={Link} to="/profile">
          <UserProfileIcon profile={profile} />
          <FancyText ml="6px">{profile?.username}</FancyText>
        </MenuItem>

        <Divider />

        {/* Watchlist */}
        <MenuItem component={Link} to="/watchlist" sx={{ my: "6px" }}>
          <ListItemIcon>
            <TrackChangesIcon fontSize="small" />
          </ListItemIcon>
          My Watchlist
        </MenuItem>

        <Divider />

        {/* Notifications */}
        <MenuItem component={Link} to="/notifications" sx={{ my: "6px" }}>
          <ListItemIcon>
            <NotificationsIcon fontSize="small" />
          </ListItemIcon>
          <NotificationsBadge sx={{ "& .MuiBadge-badge": { right: -8 } }}>
            Notifications
          </NotificationsBadge>
        </MenuItem>

        {/* Tasks */}
        <TasksMenuItem />

        <Divider />

        {/* Color mode button not shown on mobile, instead part of user context menu */}
        {isMobile && (
          <MenuItem
            onClick={() => setMode(currentMode === "dark" ? "light" : "dark")}
            sx={{ my: "6px", textTransform: "capitalize" }}
          >
            <ListItemIcon>
              {currentMode === "dark" ? (
                <LightModeIcon fontSize="small" />
              ) : (
                <DarkModeIcon fontSize="small" />
              )}
            </ListItemIcon>
            {currentMode == "dark" ? "light" : "dark"} Mode
          </MenuItem>
        )}

        {/* Switch Profile */}
        <MenuItem onClick={handleSwitchProfile} sx={{ my: "6px" }}>
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
