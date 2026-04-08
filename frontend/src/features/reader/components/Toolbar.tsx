import { Stack, Typography } from "@mui/material";
import React, { useEffect, useState } from "react";

import { ResponsiveDrawerAppBar } from "../../../shared/components/base/ResponsiveDrawer";
import type { BookContent } from "../utils/BookContentModels";
import { Settings } from "./settings/Settings";

interface ToolbarProps {
  content: BookContent;
  isCompactLayout: boolean;
  sidebarOpen: boolean;
  openSidebar: () => void;
}

export const Toolbar: React.FC<ToolbarProps> = ({
  content,
  isCompactLayout,
  sidebarOpen,
  openSidebar,
}) => {
  const [visible, setVisible] = useState(true);

  useEffect(() => {
    setVisible(true);
    const timer = setTimeout(() => setVisible(false), 3000);
    return () => clearTimeout(timer);
  }, []);

  return (
    <ResponsiveDrawerAppBar
      open={sidebarOpen}
      toggle={openSidebar}
      disableMobileTopInset
      mobileOverride={isCompactLayout}
    >
      <Stack direction="row" alignItems="center" flex={1} minWidth={0}>
        <Stack
          direction={isCompactLayout ? "row" : { sm: "column", md: "row" }}
          spacing={1}
          alignItems={
            isCompactLayout ? "center" : { sm: "stretch", md: "baseline" }
          }
          flex={1}
          minWidth={0}
          overflow="hidden"
          sx={{
            opacity: isCompactLayout && !visible ? 0 : 1,
            transition: "opacity 0.5s ease",
          }}
        >
          <Typography
            variant="h6"
            noWrap
            component="div"
            pr={isCompactLayout ? 0 : 1}
            minWidth={0}
            color="textPrimary"
            textAlign="left"
          >
            {content.metadata.title}
          </Typography>
          <Typography
            variant="body2"
            color="text.secondary"
            noWrap
            component="div"
          >
            {content.metadata.subtitle}
          </Typography>
        </Stack>
        <Settings />
      </Stack>
    </ResponsiveDrawerAppBar>
  );
};
