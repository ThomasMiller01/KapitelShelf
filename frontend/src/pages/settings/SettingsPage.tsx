import { Box, Typography } from "@mui/material";
import { type ReactElement } from "react";

import { MobileSettings } from "../../mobile/MobileSettingsFeature";
import { IsMobileApp } from "../../utils/MobileUtils";

export const SettingsPage = (): ReactElement => (
  <Box padding="20px">
    <Typography variant="h5">Settings</Typography>
    {IsMobileApp() && <MobileSettings />}
  </Box>
);
