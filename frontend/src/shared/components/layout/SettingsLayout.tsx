import { Box } from "@mui/material";
import type { ReactElement } from "react";
import { Outlet } from "react-router-dom";

export const SettingsLayout = (): ReactElement => (
  <Box>
    <Outlet />
  </Box>
);
