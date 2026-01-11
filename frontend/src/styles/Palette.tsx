import type { PaletteOptions } from "@mui/material";

export const PaletteDark: PaletteOptions = {
  mode: "dark",
  primary: {
    main: "#F7E8C9",
  },
  secondary: {
    main: "#B59E7D",
  },
  background: {
    default: "#121212",
    paper: "#1E1E1E",
  },
  text: {
    primary: "#F7E8C9",
    secondary: "#BBBBBB",
  },
  error: {
    main: "#CF6679",
  },
};

export const PaletteLight: PaletteOptions = {
  mode: "light",

  // accents stay muted and secondary
  primary: {
    main: "#6B7C82", // desaturated blue-gray
  },

  secondary: {
    main: "#A8895B", // warm muted beige-gold
  },

  // this is where the theme lives
  background: {
    default: "#EFEAE2", // warm linen / desk
    paper: "#FCF9F4", // clean book page
  },

  // text must carry contrast
  text: {
    primary: "#2A2621", // warm ink
    secondary: "#6B6258", // readable, soft
  },

  divider: "#DED6C9",

  error: {
    main: "#C44536", // warm red, not alarming
  },
};
