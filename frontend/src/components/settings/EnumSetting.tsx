import {
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  type SelectChangeEvent,
} from "@mui/material";
import React from "react";

import type { TypeSettingProps } from "./SettingItem";

export type EnumSettingProps = TypeSettingProps & {
  options?: Array<{
    value: string;
    label: React.ReactNode;
    description?: React.ReactNode;
  }>;
};

export const EnumSetting: React.FC<EnumSettingProps> = ({
  setting,
  label,
  update,
  options,
}) => {
  const value: string = (setting.value as string | undefined) ?? "";
  const id: string =
    typeof label === "string" ? label : setting.key ?? "enum-setting";

  const handleChange = (event: SelectChangeEvent<string>): void => {
    update(event.target.value);
  };

  return (
    <FormControl size="small" variant="filled" sx={{ minWidth: 220 }}>
      <InputLabel id={`${id}-label`}>{label}</InputLabel>
      <Select
        labelId={`${id}-label`}
        value={value}
        label={typeof label === "string" ? label : undefined}
        onChange={handleChange}
      >
        {(options ?? []).map((o) => (
          <MenuItem key={o.value} value={o.value}>
            {o.label}
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  );
};
