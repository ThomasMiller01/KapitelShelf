import {
  Box,
  CssBaseline,
  Toolbar,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import { styled } from "@mui/material/styles";
import type { ReactElement } from "react";
import { useEffect, useState } from "react";
import { Outlet } from "react-router-dom";

import { AppBar } from "../AppBar";
import { DRAWER_WIDTH } from "../base/ResponsiveDrawer";
import { Sidebar } from "../Sidebar";

const Main = styled("main", {
  shouldForwardProp: (prop) => prop !== "open" && prop !== "mobile",
})<{ open: boolean; mobile: boolean }>(({ theme, open, mobile }) => ({
  flexGrow: 1,
  padding: theme.spacing(3),
  transition: theme.transitions.create("margin", {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.leavingScreen,
  }),
  marginLeft: mobile ? 0 : `-${DRAWER_WIDTH}px`,
  ...(open && !mobile && { marginLeft: 0 }),
}));

export const MainLayout = (): ReactElement => {
  const theme = useTheme();
  const isMobile: boolean = useMediaQuery(theme.breakpoints.down("md"));
  const [open, setOpen] = useState<boolean>(!isMobile);

  useEffect(() => {
    setOpen(!isMobile);
  }, [isMobile]);

  const toggleDrawer = (): void => {
    setOpen((prev) => !prev);
  };

  return (
    <Box sx={{ display: "flex" }}>
      <CssBaseline />
      <Sidebar open={open} onClose={toggleDrawer} mobile={isMobile} />
      <AppBar open={open} mobile={isMobile} toggle={toggleDrawer} />
      <Main open={open} mobile={isMobile}>
        <Toolbar />
        <Outlet />
      </Main>
    </Box>
  );
};
