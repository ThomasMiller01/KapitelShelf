import TuneIcon from "@mui/icons-material/Tune";
import { Box, styled, SwipeableDrawer } from "@mui/material";
import { useState } from "react";

import { IconButtonWithTooltip } from "../../../components/base/IconButtonWithTooltip";
import { ThemeSelector } from "./settings/ThemeSelector";

const Puller = styled("div")(({ theme }) => ({
  width: 30,
  height: 6,
  borderRadius: 3,
  backgroundColor: theme.palette.text.secondary,
  position: "absolute",
  top: 8,
  left: "calc(50% - 15px)",
}));

export const Settings: React.FC = () => {
  const [open, setOpen] = useState(false);
  return (
    <>
      <IconButtonWithTooltip tooltip="Settings" onClick={() => setOpen(true)}>
        <TuneIcon />
      </IconButtonWithTooltip>
      <SwipeableDrawer
        open={open}
        onOpen={() => setOpen(true)}
        onClose={() => setOpen(false)}
        anchor="bottom"
        slotProps={{
          paper: {
            sx: {
              borderTopLeftRadius: 8,
              borderTopRightRadius: 8,
            },
          },
        }}
      >
        <Puller />
        <Box sx={{ p: 2, pb: 2, height: "100%", overflow: "auto" }}>
          <ThemeSelector />
        </Box>
      </SwipeableDrawer>
    </>
  );
};
