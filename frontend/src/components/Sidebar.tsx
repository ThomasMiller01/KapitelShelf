import CloudQueueIcon from "@mui/icons-material/CloudQueue";
import DevicesIcon from "@mui/icons-material/Devices";
import HomeIcon from "@mui/icons-material/Home";
import LibraryBooksIcon from "@mui/icons-material/LibraryBooks";
import type { SxProps, Theme } from "@mui/material";
import {
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Stack,
} from "@mui/material";
import type { ReactElement, ReactNode } from "react";
import { NavLink } from "react-router-dom";

import { useVersion } from "../hooks/useVersion";
import { ResponsiveDrawer } from "./base/ResponsiveDrawer";

interface SidebarProps {
  open: boolean;
  mobile: boolean;
  onClose: () => void;
}

export const Sidebar = ({
  open,
  onClose,
  mobile,
}: SidebarProps): ReactElement => {
  const { APP_VERSION, BACKEND_VERSION } = useVersion();

  return (
    <ResponsiveDrawer
      open={open}
      mobile={mobile}
      onClose={onClose}
      name="KapitelShelf"
      logo="/kapitelshelf.png"
    >
      <Stack justifyContent="space-between" height="100%">
        <List>
          <SidebarLinkItem
            name="Home"
            icon={<HomeIcon />}
            link="/"
            onClose={mobile ? onClose : undefined}
          />
          <SidebarLinkItem
            name="Library"
            icon={<LibraryBooksIcon />}
            link="/library"
            onClose={mobile ? onClose : undefined}
          />
        </List>
        <List>
          <SidebarTextItem
            name={`Frontend v${APP_VERSION}`}
            icon={<DevicesIcon />}
            small
          />
          <SidebarTextItem
            name={`Backend v${BACKEND_VERSION}`}
            icon={<CloudQueueIcon />}
            small
          />
        </List>
      </Stack>
    </ResponsiveDrawer>
  );
};

interface SidebarItemProps {
  name: string;
  icon?: ReactNode;
}

interface InternalSidebarItemProps {
  small?: boolean;
  children: ReactNode;
  sx?: SxProps<Theme>;
}

const SidebarItem = ({
  small = false,
  children,
  sx,
}: InternalSidebarItemProps): ReactElement => (
  <ListItem dense={small} sx={{ padding: "4px 10px", ...sx }}>
    {children}
  </ListItem>
);

interface SidebarLinkItemProps extends SidebarItemProps {
  link: string;
  onClose?: () => void;
}

const SidebarLinkItem = ({
  name,
  icon,
  link,
  onClose = undefined,
}: SidebarLinkItemProps): ReactElement => (
  <SidebarItem>
    <ListItemButton
      component={NavLink}
      to={link}
      onClick={onClose}
      sx={{
        borderRadius: "20px",
        "&.active": {
          backgroundColor: "action.selected", // or a custom color
        },
      }}
    >
      <ListItemIcon>{icon}</ListItemIcon>
      <ListItemText primary={name} />
    </ListItemButton>
  </SidebarItem>
);

interface SidebarTextItemProps extends SidebarItemProps {
  small?: boolean;
}

const SidebarTextItem = ({
  name,
  icon,
  small = false,
}: SidebarTextItemProps): ReactElement => (
  <SidebarItem
    small={small}
    sx={{
      padding: small ? "0px 16px" : "8px 16px",
      margin: small ? "0" : "4px",
    }}
  >
    {icon && (
      <ListItemIcon sx={{ minWidth: "40px", "& svg": { fontSize: "1.4rem" } }}>
        {icon}
      </ListItemIcon>
    )}
    <ListItemText primary={name} />
  </SidebarItem>
);
