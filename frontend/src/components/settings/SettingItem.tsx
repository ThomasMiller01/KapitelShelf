/* eslint-disable @typescript-eslint/no-explicit-any */
import { Box, Stack, Typography } from "@mui/material";
import type { ReactNode } from "react";
import React from "react";

import type { ObjectSettingsDTO } from "../../lib/api/KapitelShelf.Api";
import { useUpdateSetting } from "../../lib/requests/settings/useUpdateSetting";
import { BooleanSetting } from "./BooleanSetting";

type SettingType = "boolean";

interface SettingItemProps {
  setting: ObjectSettingsDTO | undefined;
  type: SettingType;
  label: ReactNode;
  description?: string;
  details?: ReactNode;
}

export const SettingItem: React.FC<SettingItemProps> = ({
  setting,
  description,
  details,
  ...props
}) => {
  const { mutate: updateSetting } = useUpdateSetting(setting);

  if (setting === undefined) {
    return <></>;
  }

  return (
    <Stack
      direction={{ xs: "column", md: "row" }}
      alignItems={{ xs: "start", md: "center" }}
      spacing={{ xs: 1, md: 1.5 }}
      sx={{ my: "5px" }}
    >
      <SpecificSettingItem
        setting={setting}
        {...props}
        update={(value: any) => updateSetting(value)}
      />
      {description && (
        <Typography variant="subtitle2" sx={{ mt: "3px !important" }}>
          {description}
        </Typography>
      )}
      <Box>{details}</Box>
    </Stack>
  );
};

type SpecificItemProps = SettingItemProps & {
  setting: ObjectSettingsDTO;
  update: (value: any) => void;
};

const SpecificSettingItem: React.FC<SpecificItemProps> = ({
  type,
  ...props
}) => {
  switch (type) {
    case "boolean":
      return <BooleanSetting {...props} />;

    default:
      return <></>;
  }
};

export type TypeSettingProps = Omit<
  SpecificItemProps,
  "type" | "description" | "details"
>;
