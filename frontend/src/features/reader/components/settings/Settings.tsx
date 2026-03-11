import TuneIcon from "@mui/icons-material/Tune";
import { Box, styled, SwipeableDrawer, Tab, Tabs } from "@mui/material";
import { useState } from "react";

import { IconButtonWithTooltip } from "../../../../components/base/IconButtonWithTooltip";
import { IsMobileApp } from "../../../../utils/MobileUtils";
import { FontSizeSelector } from "./FontSizeSelector";
import { ThemeSelector } from "./ThemeSelector";

const Puller = styled("div")(({ theme }) => ({
  width: 30,
  height: 6,
  borderRadius: 3,
  backgroundColor: theme.palette.text.secondary,
  position: "absolute",
  top: 8,
  left: "calc(50% - 15px)",
}));

const SettingsTab = styled(Tab)({
  minHeight: 32,
  minWidth: 72,
  paddingLeft: 8,
  paddingRight: 8,
  paddingTop: 4,
  paddingBottom: 4,
  fontSize: "0.8rem",
});

interface SettingsTabPanelProps {
  value: string;
  index: string;
  children?: React.ReactNode;
}

const SettingsTabPanel: React.FC<SettingsTabPanelProps> = ({
  value,
  index,
  children,
}) => {
  return (
    <Box
      role="tabpanel"
      hidden={value !== index}
      id={`settings-tabpanel-${index}`}
      aria-labelledby={`settings-tab-${index}`}
    >
      {value === index && <Box sx={{ p: 1 }}>{children}</Box>}
    </Box>
  );
};

export const Settings: React.FC = () => {
  const [open, setOpen] = useState(false);
  const [tab, setTab] = useState("appearance");

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
          <Tabs
            value={tab}
            onChange={(_, value) => setTab(value)}
            sx={{
              mb: 1,
              mt: 1,
              minHeight: 32,
              borderBottom: 2,
              borderColor: "divider",
              overflow: "visible",
              "& .MuiTabs-scroller": {
                overflow: "visible !important",
              },
              "& .MuiTabs-indicator": {
                bottom: -2,
                zIndex: 10,
              },
            }}
          >
            <SettingsTab value="appearance" label="Appearance" />
            <SettingsTab value="font" label="Font" />
          </Tabs>
          <SettingsTabPanel value={tab} index="appearance">
            <ThemeSelector />
          </SettingsTabPanel>
          <SettingsTabPanel value={tab} index="font">
            <FontSizeSelector />
          </SettingsTabPanel>
        </Box>

        {/* Bottom padding for mobile */}
        {IsMobileApp() && (
          <Box height="10px" width="100%" bgcolor="background.paper" />
        )}
      </SwipeableDrawer>
    </>
  );
};
