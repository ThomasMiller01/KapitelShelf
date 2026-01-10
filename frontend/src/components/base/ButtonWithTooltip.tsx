import type { ButtonProps } from "@mui/material";
import { Button, Tooltip } from "@mui/material";
import React from "react";

interface ButtonWithTooltipProps extends ButtonProps {
  tooltip: string | undefined;
  disabledTooltip?: string;
  to?: string;
}

export const ButtonWithTooltip: React.FC<ButtonWithTooltipProps> = ({
  tooltip,
  children,
  disabled,
  disabledTooltip = undefined,
  ...props
}) => (
  <Tooltip title={disabledTooltip && disabled ? disabledTooltip : tooltip}>
    <span>
      <Button {...props} disabled={disabled}>
        {children}
      </Button>
    </span>
  </Tooltip>
);
