import { Box, CssBaseline, Toolbar } from "@mui/material";
import { styled } from "@mui/material/styles";
import type { ReactElement } from "react";
import { Outlet } from "react-router-dom";

import { useMobile } from "../../hooks/useMobile";
import { useResponsiveDrawer } from "../../hooks/useResponsiveDrawer";
import { AppBar } from "../AppBar";
import { DRAWER_WIDTH } from "../base/ResponsiveDrawer";
import { Sidebar } from "../Sidebar";

const Main = styled("main", {
  shouldForwardProp: (prop) => prop !== "open" && prop !== "isMobile",
})<{ open: boolean; isMobile: boolean }>(({ theme, open, isMobile }) => ({
  flexGrow: 1,
  transition: theme.transitions.create("margin", {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.leavingScreen,
  }),
  marginLeft: isMobile ? 0 : `-${DRAWER_WIDTH}px`,
  ...(open && !isMobile && { marginLeft: 0 }),
}));

export const MainLayout = (): ReactElement => {
  const { isMobile } = useMobile();
  const [open, toggleDrawer] = useResponsiveDrawer();

  return (
    <Box sx={{ display: "flex" }}>
      <CssBaseline />
      <Sidebar open={open} onClose={toggleDrawer} />
      <AppBar open={open} toggle={toggleDrawer} />
      <Main open={open} isMobile={isMobile}>
        <Toolbar />
        <Outlet />
      </Main>
    </Box>
  );
};
