import { Box, Stack, Tooltip, Typography } from "@mui/material";
import type { ReactElement } from "react";

interface PropertyProps {
  children: ReactElement | string | undefined;
  label?: string;
  tooltip?: string | null;
  width?: string;
}

export const Property: React.FC<PropertyProps> = ({
  children,
  label,
  tooltip,
  width = "fit-content",
}) => (
  <Tooltip
    title={tooltip}
    disableFocusListener={!tooltip}
    disableHoverListener={!tooltip}
    disableInteractive={!tooltip}
    disableTouchListener={!tooltip}
    sx={{ width }}
  >
    <Stack spacing={0.3}>
      <Typography variant="subtitle2" color="text.secondary">
        {label}
      </Typography>
      <Box>{children}</Box>
    </Stack>
  </Tooltip>
);
