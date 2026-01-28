import { Alert, Divider, Paper, Stack, Typography } from "@mui/material";

import { SettingItem } from "../../components/settings/SettingItem";
import type { ObjectSettingsDTO } from "../../lib/api/KapitelShelf.Api";

interface CloudStorageSettingsProps {
  settings: ObjectSettingsDTO[];
}

export const CloudStorageSettings: React.FC<CloudStorageSettingsProps> = ({
  settings,
}) => {
  const experimentalBisync = settings.find(
    (x) => x.key === "cloudstorage.rclone.experimental-bisync",
  );

  return (
    <Paper sx={{ my: 2, py: 1.2, px: 2 }}>
      <Typography variant="h6">Cloud Storage</Typography>
      <Divider sx={{ mb: 2 }} />
      <SettingItem
        setting={experimentalBisync}
        type="boolean"
        label={
          <Stack direction="row" spacing={1} alignItems="center">
            <Typography>Enable</Typography>
            <Typography
              variant="overline"
              textTransform="lowercase"
              fontSize="1rem"
            >
              rclone bisync
            </Typography>
          </Stack>
        }
        description="Allows two-way synchronization."
        details={
          <Alert
            severity="warning"
            sx={{
              padding: "0px 14px",
              height: "fit-content",
              "& .MuiAlert-message": {
                height: "fit-content",
              },
              "& .MuiAlert-icon": {
                height: "fit-content",
                svg: { width: "0.9em", height: "0.9em" },
              },
            }}
          >
            This command is experimental, use with caution.
          </Alert>
        }
      />
    </Paper>
  );
};
