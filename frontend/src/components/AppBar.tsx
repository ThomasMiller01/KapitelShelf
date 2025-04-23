import { Avatar, Box, Stack } from "@mui/material";
import type { ReactElement } from "react";

import { ThemeToggle } from "../contexts/ThemeContext";
import { ResponsiveDrawerAppBar } from "./base/ResponsiveDrawer";

interface TopAppBarProps {
  open: boolean;
  mobile: boolean;
  toggle: () => void;
}

export const AppBar = ({
  open,
  mobile,
  toggle,
}: TopAppBarProps): ReactElement => (
  <ResponsiveDrawerAppBar open={open} mobile={mobile} toggle={toggle}>
    <Box
      sx={{
        width: "100%",
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
      }}
    >
      SEARCH
      <Stack direction="row" spacing={3} alignItems="center">
        <ThemeToggle />
        <Avatar
          alt="User Avatar"
          src="/avatar.png"
          sx={{ width: 32, height: 32 }}
        />
      </Stack>
    </Box>
  </ResponsiveDrawerAppBar>
);
