import "./Fonts";

import { createTheme, responsiveFontSizes } from "@mui/material/styles";

import { PaletteDark, PaletteLight } from "./Palette";

export const theme = responsiveFontSizes(
  createTheme({
    colorSchemes: {
      light: {
        palette: PaletteLight,
      },
      dark: {
        palette: PaletteDark,
      },
    },
    shape: {
      borderRadius: 8,
    },
    breakpoints: {
      values: {
        xs: 0,
        sm: 600,
        md: 900,
        lg: 1200,
        xl: 1536,
      },
    },
  })
);
