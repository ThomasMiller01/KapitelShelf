import { Box, Stack, Tooltip, Typography } from "@mui/material";
import type { ReactElement } from "react";

interface PropertyProps {
  children: ReactElement | string | undefined | null;
  label?: string;
  tooltip?: string | null;
  width?: string;
  disabled?: boolean;
}

export const Property: React.FC<PropertyProps> = ({
  children,
  label,
  tooltip,
  width = "fit-content",
  disabled = false,
}) => (
  <Tooltip
    title={tooltip}
    disableFocusListener={!tooltip}
    disableHoverListener={!tooltip}
    disableInteractive={!tooltip}
    disableTouchListener={!tooltip}
    sx={{ width }}
  >
    <Stack
      spacing={0.3}
      sx={{ opacity: disabled ? 0.5 : 1, height: "100%", width: "fit-content" }}
    >
      <Typography variant="subtitle2" color="text.secondary">
        {label}
      </Typography>
      <Box>{children}</Box>
    </Stack>
  </Tooltip>
);
