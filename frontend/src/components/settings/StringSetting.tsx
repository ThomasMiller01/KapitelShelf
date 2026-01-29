import { TextField } from "@mui/material";
import React, { useEffect, useRef, useState } from "react";

import type { TypeSettingProps } from "./SettingItem";

export interface StringSettingProps extends TypeSettingProps {
  placeholder?: string;
}

export const StringSetting: React.FC<StringSettingProps> = ({
  setting,
  label,
  update,
  placeholder,
  enabled,
}) => {
  const initialValue: string = (setting.value as string | undefined) ?? "";

  const [localValue, setLocalValue] = useState<string>(initialValue);
  const timeoutRef = useRef<number | undefined>(undefined);

  useEffect(() => {
    setLocalValue(initialValue);
  }, [initialValue]);

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>): void => {
    const next = event.target.value;
    setLocalValue(next);

    if (timeoutRef.current !== undefined) {
      window.clearTimeout(timeoutRef.current);
    }

    timeoutRef.current = window.setTimeout(() => {
      update(next);
    }, 800);
  };

  return (
    <TextField
      size="small"
      variant="filled"
      label={typeof label === "string" ? label : undefined}
      value={localValue}
      placeholder={placeholder}
      onChange={handleChange}
      sx={{ minWidth: 260 }}
      disabled={!enabled}
    />
  );
};
