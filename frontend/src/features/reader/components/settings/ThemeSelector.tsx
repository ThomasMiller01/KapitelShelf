import CheckIcon from "@mui/icons-material/Check";
import { Box, Stack, Typography } from "@mui/material";
import React from "react";

import type { ReaderColorScheme } from "../../../../styles/Theme";
import { themeForScheme, useReaderColorScheme } from "../ThemeProvider";

const THEME_OPTIONS: ReaderColorScheme[] = ["light", "sepia", "dark"];

export const ThemeSelector: React.FC = (): React.ReactElement => {
  const { colorScheme, setColorScheme } = useReaderColorScheme();

  return (
    <Stack spacing={1}>
      <Typography variant="caption" color="text.secondary" fontSize="1rem">
        Page Color
      </Typography>
      <Stack direction="row" spacing={1.5}>
        {THEME_OPTIONS.map((option) => {
          const selected = colorScheme === option;
          return (
            <Box
              key={option}
              onClick={() => setColorScheme(option)}
              sx={(theme) => ({
                position: "relative",
                width: 44,
                height: 44,
                borderRadius: "50%",
                background: themeForScheme[option].palette.background.default,
                border: `1px solid ${theme.palette.divider}`,
                outline: selected
                  ? `3px solid ${theme.palette.divider}`
                  : "none",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                cursor: "pointer",
                transition:
                  "border-color 0.15s, outline 0.15s, transform 0.15s",
                boxShadow: "0 1px 4px rgba(0,0,0,0.18)",
                "&:hover": {
                  transform: "scale(1.08)",
                },
              })}
            >
              {selected && <CheckIcon sx={{ fontSize: 18 }} />}
            </Box>
          );
        })}
      </Stack>
    </Stack>
  );
};
