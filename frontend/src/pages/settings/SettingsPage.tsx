import { Box, Typography } from "@mui/material";
import { type ReactElement } from "react";

import { AiSettings } from "../../features/settings/ai/AiSettingsFeature";
import { CloudStorageSettings } from "../../features/settings/CloudStorageSettingsFeature";
import { MobileSettings } from "../../features/settings/MobileSettingsFeature";
import { useSettingsList } from "../../lib/requests/settings/useSettingsList";
import { IsMobileApp } from "../../utils/MobileUtils";

export const SettingsPage = (): ReactElement => {
  const { data: settings } = useSettingsList();

  return (
    <Box padding="20px">
      <Typography variant="h5">Settings</Typography>
      {IsMobileApp() && <MobileSettings />}
      <CloudStorageSettings settings={settings ?? []} />
      <AiSettings settings={settings ?? []} />
    </Box>
  );
};
