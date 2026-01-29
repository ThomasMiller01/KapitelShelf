import {
  alpha,
  FormControlLabel,
  FormGroup,
  styled,
  Switch,
} from "@mui/material";
import { pink } from "@mui/material/colors";
import React from "react";

import type { TypeSettingProps } from "./SettingItem";

const PinkSwitch = styled(Switch)(({ theme }) => ({
  "& .MuiSwitch-switchBase.Mui-checked": {
    color: pink[600],
    "&:hover": {
      backgroundColor: alpha(pink[600], theme.palette.action.hoverOpacity),
    },
  },
  "& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track": {
    backgroundColor: pink[600],
  },
}));

export const BooleanSetting: React.FC<TypeSettingProps> = ({
  setting: { value },
  label,
  update,
  enabled,
}) => (
  <FormGroup>
    <FormControlLabel
      label={label}
      control={
        <PinkSwitch
          onChange={(e) => update(e.target.checked)}
          disabled={!enabled}
        />
      }
      checked={value === true}
      sx={{ mr: "10px" }}
    />
  </FormGroup>
);
