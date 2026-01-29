/* eslint-disable @typescript-eslint/no-explicit-any */
import React, { useMemo } from "react";

import { Divider, Stack, Typography } from "@mui/material";
import { BooleanSetting } from "./BooleanSetting";
import { EnumSettingProps } from "./EnumSetting";

type ListStringAsBooleanProps = EnumSettingProps;

export const ListStringAsBoolean: React.FC<ListStringAsBooleanProps> = ({
  setting,
  label,
  description,
  update,
  options,
}) => {
  const enabledSet = useMemo<Set<string>>(() => {
    return new Set(setting.value);
  }, [setting.value]);

  const handleToggle = (optionValue: string, isEnabled: boolean): void => {
    const next = new Set(enabledSet);

    if (isEnabled) {
      next.add(optionValue);
    } else {
      next.delete(optionValue);
    }

    update(Array.from(next).sort());
  };

  return (
    <Stack spacing={0.5} sx={{ minWidth: "200px" }}>
      <Stack>
        <Typography variant="subtitle1">{label}</Typography>
        <Typography variant="body2" fontStyle="italic">
          {description}
        </Typography>
      </Stack>
      <Divider sx={{ mb: 2 }} />
      {options?.map((option) => {
        const checked = enabledSet.has(option.value);

        return (
          <BooleanSetting
            key={option.value}
            setting={{ value: checked }}
            label={option.label}
            update={(x) => handleToggle(option.value, x === true)}
          />
        );
      })}
    </Stack>
  );
};
