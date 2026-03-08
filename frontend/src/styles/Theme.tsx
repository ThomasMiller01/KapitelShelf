import "./Fonts";

import {
  createTheme,
  PaletteOptions,
  responsiveFontSizes,
} from "@mui/material/styles";

import { PaletteDark, PaletteLight, PaletteSepia } from "./Palette";

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
  }),
);

export type ReaderColorScheme = "light" | "dark" | "sepia";

export const createReaderTheme = (colorscheme: ReaderColorScheme) => {
  let palette: PaletteOptions | undefined = undefined;
  switch (colorscheme) {
    case "light":
      palette = PaletteLight;
      break;
    case "dark":
      palette = PaletteDark;
      break;
    case "sepia":
      palette = PaletteSepia;
      break;
  }

  return responsiveFontSizes(
    createTheme({
      palette,
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
    }),
  );
};
