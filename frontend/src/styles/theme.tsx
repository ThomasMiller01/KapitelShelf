import type { Theme } from "@mui/material/styles";
import { createTheme, responsiveFontSizes } from "@mui/material/styles";

import { palette } from "./palette";

const GetTheme = (): Theme => {
  let theme = createTheme({
    palette,
    shape: {
      borderRadius: 8,
    },
    components: {
      MuiButton: {
        defaultProps: {
          disableElevation: true,
        },
      },
    },
  });
  theme = responsiveFontSizes(theme);
  return theme;
};

export default GetTheme;
