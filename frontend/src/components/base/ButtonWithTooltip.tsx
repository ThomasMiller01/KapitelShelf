import type { ButtonProps } from "@mui/material";
import { Button, Tooltip } from "@mui/material";
import React from "react";

interface ButtonWithTooltipProps extends ButtonProps {
  tooltip: string | undefined;
  to?: string;
}

export const ButtonWithTooltip: React.FC<ButtonWithTooltipProps> = ({
  tooltip,
  children,
  ...props
}) => (
  <Tooltip title={tooltip}>
    <Button {...props}>{children}</Button>
  </Tooltip>
);
