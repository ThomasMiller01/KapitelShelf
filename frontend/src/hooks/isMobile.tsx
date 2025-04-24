import { useMediaQuery, useTheme } from "@mui/material";

interface MobileResult {
  isMobile: boolean;
}

export const useMobile = (): MobileResult => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("md"));

  return { isMobile };
};
