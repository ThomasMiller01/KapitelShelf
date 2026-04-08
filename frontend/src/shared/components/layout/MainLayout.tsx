import { Box, Toolbar } from "@mui/material";
import { styled } from "@mui/material/styles";
import type { ReactElement } from "react";
import { Outlet } from "react-router-dom";

import { useMobile } from "../../hooks/useMobile";
import { useResponsiveDrawer } from "../../hooks/useResponsiveDrawer";
import {
  IsMobileApp,
  MOBILE_APP_BOTTOM_INSET,
  MOBILE_APP_TOP_INSET,
} from "../../utils/MobileUtils";
import { AppBar } from "../AppBar";
import { DRAWER_WIDTH } from "../base/ResponsiveDrawer";
import { Sidebar } from "../Sidebar";

const Main = styled("main", {
  shouldForwardProp: (prop) => prop !== "open" && prop !== "isMobile",
})<{ open: boolean; isMobile: boolean }>(({ theme, open, isMobile }) => ({
  flexGrow: 1,
  minWidth: 0,
  transition: theme.transitions.create("margin", {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.leavingScreen,
  }),
  marginLeft: isMobile ? 0 : `-${DRAWER_WIDTH}px`,
  ...(open && !isMobile && { marginLeft: 0 }),
}));

interface MainLayoutProps {
  minimal?: boolean;
  disableMobileTopInset?: boolean;
}

export const MainLayout: React.FC<MainLayoutProps> = ({
  minimal = false,
  disableMobileTopInset = false,
}) => {
  const { isMobile } = useMobile();
  const [open, toggleDrawer] = useResponsiveDrawer();

  if (minimal) {
    return (
      <MainLayoutWrapper disableMobileTopInset={disableMobileTopInset}>
        <Outlet />
      </MainLayoutWrapper>
    );
  }

  return (
    <MainLayoutWrapper disableMobileTopInset={disableMobileTopInset}>
      <Sidebar open={open} onClose={toggleDrawer} />
      <AppBar open={open} toggle={toggleDrawer} />
      <Main open={open} isMobile={isMobile}>
        <Toolbar />
        <Outlet />
      </Main>
    </MainLayoutWrapper>
  );
};

interface MainLayoutWrapperProps {
  children: ReactElement | ReactElement[];
  disableMobileTopInset?: boolean;
}

const MainLayoutWrapper: React.FC<MainLayoutWrapperProps> = ({
  children,
  disableMobileTopInset = false,
}) => {
  return (
    <Box
      sx={{
        display: "flex",
        minWidth: 0,
        paddingTop:
          IsMobileApp() && !disableMobileTopInset ? MOBILE_APP_TOP_INSET : 0,
        paddingBottom: IsMobileApp() ? MOBILE_APP_BOTTOM_INSET : 0,
      }}
    >
      {children}
    </Box>
  );
};
