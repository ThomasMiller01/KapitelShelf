/* eslint-disable @typescript-eslint/no-explicit-any */
import { Box, Stack, Typography } from "@mui/material";
import type { ReactNode } from "react";
import React from "react";

import type { ObjectSettingsDTO } from "../../lib/api/KapitelShelf.Api";
import { useUpdateSetting } from "../../lib/requests/settings/useUpdateSetting";
import { BooleanSetting } from "./BooleanSetting";
import { EnumSetting } from "./EnumSetting";
import { ListStringAsBoolean } from "./ListStringAsBoolean";
import { StringSetting } from "./StringSetting";

type SettingType = "boolean" | "enum" | "string" | "list{string}-as-boolean";

const IngoreDefaultDescriptionPosition: SettingType[] = [
  "list{string}-as-boolean",
];

interface SettingItemProps {
  setting: ObjectSettingsDTO | undefined;
  type: SettingType;
  label: ReactNode;
  description?: string;
  details?: ReactNode;

  // enum & list{string}-as-boolean props
  options?: Array<{ value: string; label: ReactNode; description?: ReactNode }>;

  // string-only props
  placeholder?: string;
}

export const SettingItem: React.FC<SettingItemProps> = ({
  setting,
  description,
  details,
  type,
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
      sx={{ my: "10px" }}
    >
      <SpecificSettingItem
        setting={setting}
        type={type}
        description={description}
        {...props}
        update={(value: any) => updateSetting(value)}
      />
      {description && !IngoreDefaultDescriptionPosition.includes(type) && (
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

    case "enum":
      return <EnumSetting {...props} />;

    case "string":
      return <StringSetting {...props} />;

    case "list{string}-as-boolean":
      return <ListStringAsBoolean {...props} />;

    default:
      return <></>;
  }
};

export type TypeSettingProps = Omit<SpecificItemProps, "type" | "details">;
