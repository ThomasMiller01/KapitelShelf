import type { IconButtonProps } from "@mui/material";
import { IconButton, Tooltip } from "@mui/material";
import React from "react";

interface IconButtonWithTooltipProps extends IconButtonProps {
  tooltip: string | undefined;

  // Link component
  to?: string;
}

export const IconButtonWithTooltip: React.FC<IconButtonWithTooltipProps> = ({
  tooltip,
  children,
  ...props
}) => (
  <Tooltip title={tooltip}>
    <IconButton {...props}>{children}</IconButton>
  </Tooltip>
);
