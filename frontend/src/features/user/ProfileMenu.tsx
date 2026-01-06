import PeopleIcon from "@mui/icons-material/People";
import TrackChangesIcon from "@mui/icons-material/TrackChanges";
import {
  Avatar,
  Divider,
  IconButton,
  ListItemIcon,
  Menu,
  MenuItem,
} from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import React from "react";
import { Link } from "react-router-dom";

import FancyText from "../../components/FancyText";
import { NotificationsBadge } from "../../components/notifications/NotificationsBadge";
import { NotificationsIcon } from "../../components/notifications/NotificationsIcon";
import { TasksMenuItem } from "../../components/tasks/TasksMenuItem";
import { useApi } from "../../contexts/ApiProvider";
import { useUserProfile } from "../../hooks/useUserProfile";
import {
  ProfileImageTypeDTO,
  type UserDTO,
} from "../../lib/api/KapitelShelf.Api/api";
import { SECOND_MS } from "../../utils/TimeUtils";
import { ProfileImageTypeToSrc } from "../../utils/UserProfileUtils";

export const ProfileMenu = (): ReactElement => {
  const { profile, clearProfile } = useUserProfile();
  const { clients } = useApi();

  const { data: unreadNotifications } = useQuery({
    queryKey: ["notifications-list-unread"],
    queryFn: async () => {
      if (profile?.id === undefined) {
        return;
      }

      const { data } = await clients.notifications.notificationsGet(
        profile?.id
      );
      return data.filter((x) => !x.isRead);
    },
    refetchInterval: 30 * SECOND_MS,
  });

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
      <NotificationsBadge
        notifications={unreadNotifications ?? []}
        overlap="circular"
      >
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
        <MenuItem component={Link} to="/profile">
          <UserProfileIcon profile={profile} />
          <FancyText ml="6px">{profile?.username}</FancyText>
        </MenuItem>
        <Divider />
        <MenuItem component={Link} to="/watchlist" sx={{ my: "6px" }}>
          <ListItemIcon>
            <TrackChangesIcon fontSize="small" />
          </ListItemIcon>
          My Watchlist
        </MenuItem>
        <Divider />
        <MenuItem component={Link} to="/notifications" sx={{ my: "6px" }}>
          <ListItemIcon>
            <NotificationsIcon
              notifications={unreadNotifications ?? []}
              fontSize="small"
            />
          </ListItemIcon>
          <NotificationsBadge
            notifications={unreadNotifications ?? []}
            sx={{ "& .MuiBadge-badge": { right: -8 } }}
          >
            Notifications
          </NotificationsBadge>
        </MenuItem>
        <TasksMenuItem />
        {/* <Divider /> */}
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
