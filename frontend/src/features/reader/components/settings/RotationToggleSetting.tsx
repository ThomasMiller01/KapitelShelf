import { FormControlLabel, Stack, Switch, Typography } from "@mui/material";
import React from "react";

import { IsMobileApp } from "../../../../utils/MobileUtils";
import { useReaderColorScheme } from "../ThemeProvider";

export const RotationToggleSetting: React.FC = (): React.ReactElement | null => {
  const { readerRotationEnabled, setReaderRotationEnabled } =
    useReaderColorScheme();

  if (!IsMobileApp()) {
    return null;
  }

  return (
    <Stack spacing={1}>
      <Typography variant="caption" color="text.secondary" fontSize="1rem">
        Orientation
      </Typography>
      <FormControlLabel
        control={
          <Switch
            checked={readerRotationEnabled}
            onChange={(event) => setReaderRotationEnabled(event.target.checked)}
          />
        }
        label="Lock current orientation"
        sx={{ ml: 0 }}
      />
    </Stack>
  );
};
