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
  Typography,
} from "@mui/material";
import { styled } from "@mui/material/styles";
import type { ReactElement, ReactNode } from "react";

export const DRAWER_WIDTH = 280;

const ProductNameText = styled(Typography)({
  fontFamily: "Playwrite AU SA",
  fontWeight: "200",
  lineHeight: "2",
  fontFeatureSettings: '"calt"',
});

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
  mobile: boolean;
  children: ReactNode;
  name?: string;
  logo?: string;
}

export const ResponsiveDrawer = ({
  open,
  onClose,
  mobile,
  children,
  name,
  logo,
}: ResponsiveDrawerProps): ReactElement => (
  <Drawer
    variant={mobile ? "temporary" : "persistent"}
    anchor="left"
    open={open}
    onClose={mobile ? onClose : undefined}
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
            sx={{
              width: 40,
              objectFit: "cover",
            }}
          />
        )}
        {name && (
          <ProductNameText variant="h6" noWrap sx={{ width: "100%" }}>
            {name}
          </ProductNameText>
        )}
      </Stack>
      {!mobile && (
        <IconButton onClick={onClose}>
          <ChevronLeftIcon />
        </IconButton>
      )}
    </DrawerHeader>

    <Divider />
    {children}
  </Drawer>
);

interface TopAppBarProps {
  open: boolean;
  mobile: boolean;
  toggle: () => void;
  children: ReactNode;
}

export const ResponsiveDrawerAppBar = ({
  open,
  mobile,
  toggle,
  children,
}: TopAppBarProps): ReactElement => (
  <AppBar
    position="fixed"
    sx={{
      zIndex: (theme) => theme.zIndex.drawer + (mobile ? 0 : 1), // keep AppBar above Sidebar (removes elevation of Sidebar next to AppBar)
      width: open && !mobile ? `calc(100% - ${DRAWER_WIDTH}px)` : "100%",
      ml: open && !mobile ? `${DRAWER_WIDTH}px` : 0,
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
        sx={{ mr: 2, ...(open && !mobile && { display: "none" }) }}
      >
        <MenuIcon />
      </IconButton>
      {children}
    </Toolbar>
  </AppBar>
);
