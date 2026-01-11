import ChevronLeftIcon from "@mui/icons-material/ChevronLeft";
import MenuIcon from "@mui/icons-material/Menu";
import {
  AppBar,
  Box,
  Divider,
  Drawer,
  IconButton,
  Stack,
  Toolbar,
} from "@mui/material";
import { styled } from "@mui/material/styles";
import type { ReactElement, ReactNode } from "react";

import { useMobile } from "../../hooks/useMobile";
import FancyText from "../FancyText";

export const DRAWER_WIDTH = 280;

const DrawerHeader = styled("div")(({ theme }) => ({
  display: "flex",
  alignItems: "center",
  justifyContent: "flex-end",
  padding: theme.spacing(0, 1),
  ...theme.mixins.toolbar,
}));

export interface ResponsiveDrawerProps {
  open: boolean;
  onClose: () => void;
  children: ReactNode;
  name?: string;
  logo?: string;
}

export const ResponsiveDrawer = ({
  open,
  onClose,
  children,
  name,
  logo,
}: ResponsiveDrawerProps): ReactElement => {
  const { isMobile } = useMobile();
  return (
    <Drawer
      variant={isMobile ? "temporary" : "persistent"}
      anchor="left"
      open={open}
      onClose={isMobile ? onClose : undefined}
      slotProps={{
        paper: {
          elevation: 4,
        },
      }}
      sx={{
        width: DRAWER_WIDTH,
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width: DRAWER_WIDTH,
          boxSizing: "border-box",
        },
      }}
    >
      <DrawerHeader sx={{ justifyContent: "space-between" }}>
        <Stack direction="row" spacing={1} alignItems="center">
          {logo && (
            <Box
              component="img"
              src={logo}
              alt="My Image"
              sx={(theme) => ({
                width: 40,
                objectFit: "cover",
                filter:
                  theme.palette.mode === "light"
                    ? "drop-shadow(0 0 6px rgba(0,0,0,0.75)) " +
                      "drop-shadow(0 0 14px rgba(0,0,0,0.55)) " +
                      "drop-shadow(0 0 26px rgba(0,0,0,0.35))"
                    : "none",
              })}
            />
          )}
          {name && (
            <FancyText variant="h6" noWrap sx={{ width: "100%" }}>
              {name}
            </FancyText>
          )}
        </Stack>
        {!isMobile && (
          <IconButton onClick={onClose}>
            <ChevronLeftIcon />
          </IconButton>
        )}
      </DrawerHeader>

      <Divider />
      {children}
    </Drawer>
  );
};

interface TopAppBarProps {
  open: boolean;
  toggle: () => void;
  children: ReactNode;
}

export const ResponsiveDrawerAppBar = ({
  open,
  toggle,
  children,
}: TopAppBarProps): ReactElement => {
  const { isMobile } = useMobile();

  return (
    <AppBar
      position="fixed"
      sx={{
        zIndex: (theme) => theme.zIndex.drawer + (isMobile ? 0 : 1), // keep AppBar above Sidebar (removes elevation of Sidebar next to AppBar)
        width: open && !isMobile ? `calc(100% - ${DRAWER_WIDTH}px)` : "100%",
        ml: open && !isMobile ? `${DRAWER_WIDTH}px` : 0,
        bgcolor: "background.paper",
        paddingRight: "0 !important",
        transition: (theme) =>
          theme.transitions.create(["margin", "width"], {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.leavingScreen,
          }),
      }}
    >
      <Toolbar>
        <IconButton
          color="inherit"
          edge="start"
          onClick={toggle}
          sx={{ mr: 2, ...(open && !isMobile && { display: "none" }) }}
        >
          <MenuIcon />
        </IconButton>
        {children}
      </Toolbar>
    </AppBar>
  );
};
