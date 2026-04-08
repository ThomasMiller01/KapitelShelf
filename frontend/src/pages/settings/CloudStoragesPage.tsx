import { Box, Typography } from "@mui/material";
import { type ReactElement } from "react";

import { OneDriveSettings } from "../../features/cloudstorage";

export const CloudStoragesPage = (): ReactElement => (
  <Box padding="20px">
    <Typography variant="h5">Cloud Storages</Typography>
    <OneDriveSettings />
  </Box>
);
