import AutoAwesomeIcon from "@mui/icons-material/AutoAwesome";
import { styled } from "@mui/material";
import React from "react";
import {
  ButtonWithTooltip,
  ButtonWithTooltipProps,
} from "./base/ButtonWithTooltip";

interface AiButtonProps extends ButtonWithTooltipProps {}

const aiBlueLight = "#0B5ED7";
const aiBlueDark = "#8FB3FF"; // lighter blue for dark mode

const AiStyledButton = styled(ButtonWithTooltip)(({ theme }) => {
  const isDark = theme.palette.mode === "dark";
  const accent = isDark ? aiBlueDark : aiBlueLight;

  return {
    borderRadius: 8,
    border: `1px solid ${accent}`,

    background: isDark
      ? `linear-gradient(135deg, rgba(91,140,255,0.25), rgba(91,140,255,0.08))`
      : `linear-gradient(135deg, rgba(11,94,215,0.18), rgba(11,94,215,0.05))`,

    color: accent,

    // override contained variant
    "&.MuiButton-contained": {
      color: accent,
    },

    "&:hover": {
      background: isDark
        ? `linear-gradient(135deg, rgba(91,140,255,0.35), rgba(91,140,255,0.15))`
        : `linear-gradient(135deg, rgba(11,94,215,0.26), rgba(11,94,215,0.10))`,
    },

    "& .MuiButton-startIcon": {
      color: accent,
    },

    "&.Mui-disabled": {
      opacity: 0.6,
      borderColor: isDark ? "rgba(91,140,255,0.4)" : "rgba(11,94,215,0.35)",
      color: theme.palette.text.disabled,
    },
  };
});

export const AiButton: React.FC<AiButtonProps> = ({ children, ...props }) => (
  <AiStyledButton
    variant="contained"
    size="small"
    startIcon={<AutoAwesomeIcon />}
    {...props}
  >
    {children}
  </AiStyledButton>
);
