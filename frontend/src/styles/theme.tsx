import type { Theme } from "@mui/material/styles";
import { createTheme, responsiveFontSizes } from "@mui/material/styles";

import { PaletteDark, PaletteLight } from "./Palette";

export const getTheme = (mode: "light" | "dark"): Theme => {
  let theme = createTheme({
    palette: mode === "dark" ? PaletteDark : PaletteLight,
    shape: {
      borderRadius: 8,
    },
  });
  theme = responsiveFontSizes(theme);
  return theme;
};
