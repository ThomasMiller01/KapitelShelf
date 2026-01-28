import EditIcon from "@mui/icons-material/Edit";
import type { ReactElement } from "react";

import { Box } from "@mui/material";
import { NavLink } from "react-router-dom";
import { FloatingButtonWithTooltip } from "../components/base/FloatingButtonWithTooltip";
import SeriesList from "../features/series/SeriesList";
import { useMobile } from "../hooks/useMobile";
import { IsMobileApp } from "../utils/MobileUtils";

const LibraryPage = (): ReactElement => {
  const { isMobile } = useMobile();
  return (
    <Box>
      <SeriesList />
      <FloatingButtonWithTooltip
        component={NavLink}
        to="/settings/manage-library"
        tooltip="Manage your Library"
        size={isMobile ? "medium" : "large"}
        sx={{
          position: "fixed",
          right: 24,
          bottom: 24 + (IsMobileApp() ? 10 : 0),
          zIndex: (theme) => theme.zIndex.modal + 1,
        }}
      >
        <EditIcon />
      </FloatingButtonWithTooltip>
    </Box>
  );
};

export default LibraryPage;
