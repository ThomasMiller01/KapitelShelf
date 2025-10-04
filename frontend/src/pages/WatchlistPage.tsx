import { Box, Typography } from "@mui/material";
import { type ReactElement } from "react";

import Watchlist from "../features/watchlist/Watchlist";
import { useMobile } from "../hooks/useMobile";

export const WatchlistPage = (): ReactElement => {
  const { isMobile } = useMobile();
  return (
    <Box padding={isMobile ? "10px" : "20px"}>
      <Typography variant="h5">Watchlist</Typography>
      <Watchlist />
    </Box>
  );
};
