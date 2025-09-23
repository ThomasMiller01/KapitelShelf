import { Box, Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { type ReactElement } from "react";

import { useApi } from "../../contexts/ApiProvider";
import { MobileSettings } from "../../features/settings/MobileSettingsFeature";
import { RCloneSettings } from "../../features/settings/RCloneSettingsFeature";
import { IsMobileApp } from "../../utils/MobileUtils";

export const SettingsPage = (): ReactElement => {
  const { clients } = useApi();
  const { data: settings } = useQuery({
    queryKey: ["settings-list"],
    queryFn: async () => {
      const { data } = await clients.settings.settingsGet();
      return data;
    },
  });

  return (
    <Box padding="20px">
      <Typography variant="h5">Settings</Typography>
      {IsMobileApp() && <MobileSettings />}
      <RCloneSettings settings={settings ?? []} />
    </Box>
  );
};
