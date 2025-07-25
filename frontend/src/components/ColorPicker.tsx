import CheckIcon from "@mui/icons-material/Check";
import { Box, IconButton } from "@mui/material";
import React from "react";

import { UserProfileColors } from "../utils/UserProfileUtils";

type ColorPickerProps = {
  value: string;
  onChange: (color: string) => void;
  size?: number; // diameter in px
};

export const ColorPicker: React.FC<ColorPickerProps> = ({
  value,
  onChange,
  size = 36,
}) => (
  <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap" }}>
    {UserProfileColors.map((color) => (
      <IconButton
        key={color}
        onClick={() => onChange(color)}
        sx={{
          width: size,
          height: size,
          backgroundColor: color,
          border: value === color ? "2px solid #222" : "2px solid transparent",
          borderRadius: "8px",
          transition: "border 0.2s",
          "&:hover": {
            border: "2px solid #555",
          },
          position: "relative",
        }}
      >
        {value === color && (
          <CheckIcon
            sx={{
              color: "#fff",
              fontSize: size * 0.6,
              position: "absolute",
              left: "50%",
              top: "50%",
              transform: "translate(-50%, -50%)",
              pointerEvents: "none",
            }}
          />
        )}
      </IconButton>
    ))}
  </Box>
);
