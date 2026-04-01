import Battery20Icon from "@mui/icons-material/Battery20";
import Battery30Icon from "@mui/icons-material/Battery30";
import Battery50Icon from "@mui/icons-material/Battery50";
import Battery60Icon from "@mui/icons-material/Battery60";
import Battery80Icon from "@mui/icons-material/Battery80";
import Battery90Icon from "@mui/icons-material/Battery90";
import BatteryCharging20Icon from "@mui/icons-material/BatteryCharging20";
import BatteryCharging30Icon from "@mui/icons-material/BatteryCharging30";
import BatteryCharging50Icon from "@mui/icons-material/BatteryCharging50";
import BatteryCharging60Icon from "@mui/icons-material/BatteryCharging60";
import BatteryCharging80Icon from "@mui/icons-material/BatteryCharging80";
import BatteryCharging90Icon from "@mui/icons-material/BatteryCharging90";
import BatteryChargingFullIcon from "@mui/icons-material/BatteryChargingFull";
import BatteryFullIcon from "@mui/icons-material/BatteryFull";
import { Stack, Typography, type SvgIconProps } from "@mui/material";
import React from "react";

export interface BatteryStatusProps {
  batteryPercent: number | null;
  isCharging?: boolean;
  fontScale: number;
}

const getBatteryIcon = (
  batteryPercent: number,
  isCharging?: boolean,
): React.ElementType<SvgIconProps> => {
  if (batteryPercent <= 14) {
    return isCharging ? BatteryCharging20Icon : Battery20Icon;
  }

  if (batteryPercent <= 29) {
    return isCharging ? BatteryCharging30Icon : Battery30Icon;
  }

  if (batteryPercent <= 44) {
    return isCharging ? BatteryCharging50Icon : Battery50Icon;
  }

  if (batteryPercent <= 59) {
    return isCharging ? BatteryCharging60Icon : Battery60Icon;
  }

  if (batteryPercent <= 74) {
    return isCharging ? BatteryCharging80Icon : Battery80Icon;
  }

  if (batteryPercent <= 89) {
    return isCharging ? BatteryCharging90Icon : Battery90Icon;
  }

  return isCharging ? BatteryChargingFullIcon : BatteryFullIcon;
};

export const BatteryStatus: React.FC<BatteryStatusProps> = ({
  batteryPercent,
  isCharging,
  fontScale,
}): React.ReactElement | null => {
  if (batteryPercent === null) {
    return null;
  }

  const boundedPercent = Math.min(100, Math.max(0, batteryPercent));
  const Icon = getBatteryIcon(boundedPercent, isCharging);

  return (
    <Stack direction="row" alignItems="center" spacing={0.4}>
      <Icon
        sx={{
          color: "text.secondary",
          fontSize: `${1.3 * fontScale}rem`,
          transform: "rotate(90deg)",
        }}
      />
      <Typography
        variant="caption"
        color="text.secondary"
        sx={{ fontSize: `${0.75 * fontScale}rem` }}
      >
        {boundedPercent}%
      </Typography>
    </Stack>
  );
};
