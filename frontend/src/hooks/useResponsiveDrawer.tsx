import { useMediaQuery, useTheme } from "@mui/material";
import { useEffect, useState } from "react";

export const useResponsiveDrawer = (): [boolean, () => void, boolean] => {
  const theme = useTheme();
  const isMobile: boolean = useMediaQuery(theme.breakpoints.down("md"));
  const [open, setOpen] = useState<boolean>(!isMobile);

  useEffect(() => {
    setOpen(!isMobile);
  }, [isMobile]);

  const toggleDrawer = (): void => {
    setOpen((prev) => !prev);
  };

  return [open, toggleDrawer, isMobile];
};
