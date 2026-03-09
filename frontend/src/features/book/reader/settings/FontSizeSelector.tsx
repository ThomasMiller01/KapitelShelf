import { Box, Slider, Stack, Typography } from "@mui/material";
import React from "react";

import {
  READER_FONT_SCALE_OPTIONS,
  useReaderColorScheme,
} from "../ThemeProvider";

const sliderMarks = READER_FONT_SCALE_OPTIONS.map((value) => ({
  value,
  // label: `${Math.round(value * 100)}%`,
}));

export const FontSizeSelector: React.FC = (): React.ReactElement => {
  const { fontScale, setFontScale } = useReaderColorScheme();

  return (
    <Stack spacing={1.5} mt={3}>
      <Typography variant="caption" color="text.secondary" fontSize="1rem">
        Font Size
      </Typography>
      <Box px={2} mt="0 !important">
        <Slider
          min={READER_FONT_SCALE_OPTIONS[0]}
          max={READER_FONT_SCALE_OPTIONS[READER_FONT_SCALE_OPTIONS.length - 1]}
          step={null}
          marks={sliderMarks}
          value={fontScale}
          onChange={(_, value) => {
            if (typeof value === "number") {
              setFontScale(value);
            }
          }}
        />
      </Box>
    </Stack>
  );
};
