import type { FabProps } from "@mui/material";
import { Fab, Tooltip } from "@mui/material";
import React from "react";

interface FloatingButtonWithTooltipProps extends FabProps {
  tooltip: string | undefined;

  // Link component
  to?: string;
}

export const FloatingButtonWithTooltip: React.FC<
  FloatingButtonWithTooltipProps
> = ({ tooltip, children, ...props }) => (
  <Tooltip title={tooltip}>
    <Fab {...props}>{children}</Fab>
  </Tooltip>
);
