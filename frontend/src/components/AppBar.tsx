import { Avatar, Box, Stack, TextField } from "@mui/material";
import type { ReactElement } from "react";

import { ThemeToggle } from "../contexts/ThemeContext";
import { ResponsiveDrawerAppBar } from "./base/ResponsiveDrawer";

interface TopAppBarProps {
  open: boolean;
  toggle: () => void;
}

export const AppBar = ({ open, toggle }: TopAppBarProps): ReactElement => (
  <ResponsiveDrawerAppBar open={open} toggle={toggle}>
    <Box
      sx={{
        width: "100%",
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
      }}
    >
      <TextField label="Search" variant="outlined" size="small" fullWidth />
      <Stack
        direction="row"
        spacing={{ xs: 2, md: 3 }}
        alignItems="center"
        ml="30px"
      >
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
