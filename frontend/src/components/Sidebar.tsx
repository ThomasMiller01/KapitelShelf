import AssignmentIcon from "@mui/icons-material/Assignment";
import CloudQueueIcon from "@mui/icons-material/CloudQueue";
import DevicesIcon from "@mui/icons-material/Devices";
import HomeIcon from "@mui/icons-material/Home";
import LibraryBooksIcon from "@mui/icons-material/LibraryBooks";
import SettingsIcon from "@mui/icons-material/Settings";
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
import { NavLink, useLocation } from "react-router-dom";

import { useMobile } from "../hooks/useMobile";
import { useVersion } from "../hooks/useVersion";
import { ResponsiveDrawer } from "./base/ResponsiveDrawer";

interface SidebarProps {
  open: boolean;
  onClose: () => void;
}

export const Sidebar = ({ open, onClose }: SidebarProps): ReactElement => {
  const { APP_VERSION, BACKEND_VERSION } = useVersion();
  const { isMobile } = useMobile();

  return (
    <ResponsiveDrawer
      open={open}
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
            onClose={isMobile ? onClose : undefined}
          />
          <SidebarLinkItem
            name="Library"
            icon={<LibraryBooksIcon />}
            link="/library"
            onClose={isMobile ? onClose : undefined}
          />
          <SidebarLinkItem
            name="Settings"
            icon={<SettingsIcon />}
            link="/settings"
            onClose={isMobile ? onClose : undefined}
            activeOnExactMatch
            showChildrenAlways={false}
          >
            <SidebarLinkItem
              name="Tasks"
              icon={<AssignmentIcon />}
              link="/settings/tasks"
              onClose={isMobile ? onClose : undefined}
              nestingLevel={1}
            />
          </SidebarLinkItem>
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
  children?: ReactElement;
  nestingLevel?: number;
  showChildrenAlways?: boolean;
  activeOnExactMatch?: boolean;
}

const SidebarLinkItem = ({
  name,
  icon,
  link,
  onClose = undefined,
  children,
  nestingLevel = 0,
  activeOnExactMatch = false,
  showChildrenAlways = true,
}: SidebarLinkItemProps): ReactElement => {
  const { pathname } = useLocation();

  return (
    <>
      <SidebarItem
        sx={{
          pl: 1.2 + 2.2 * nestingLevel,
        }}
      >
        <ListItemButton
          component={NavLink}
          to={link}
          onClick={onClose}
          end={activeOnExactMatch}
          sx={{
            borderRadius: "20px",
            "&.active": {
              backgroundColor: "action.selected", // or a custom color
            },
          }}
        >
          <ListItemIcon>{icon}</ListItemIcon>
          <ListItemText
            primary={name}
            sx={{ margin: nestingLevel === 0 ? "4px" : "0" }}
          />
        </ListItemButton>
      </SidebarItem>
      {children && (pathname.startsWith(link) || showChildrenAlways) && (
        <List sx={{ padding: "0" }}>{children}</List>
      )}
    </>
  );
};

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
