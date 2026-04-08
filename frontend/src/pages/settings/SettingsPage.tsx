import { Box, Typography } from "@mui/material";
import { type ReactElement } from "react";

import {
  AiSettings,
  CloudStorageSettings,
  MobileSettings,
  useSettingsListPoller,
} from "../../features/settings";
import { IsMobileApp } from "../../shared/utils/MobileUtils";

export const SettingsPage = (): ReactElement => {
  const { data: settings } = useSettingsListPoller();

  return (
    <Box padding="20px">
      <Typography variant="h5">Settings</Typography>
      {IsMobileApp() && <MobileSettings />}
      <CloudStorageSettings settings={settings ?? []} />
      <AiSettings settings={settings ?? []} />
    </Box>
  );
};
