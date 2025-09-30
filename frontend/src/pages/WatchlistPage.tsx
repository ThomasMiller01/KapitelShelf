import { Box, Typography } from "@mui/material";
import { type ReactElement } from "react";

import SeriesWatchlist from "../features/watchlist/SeriesWatchlist";
import { useMobile } from "../hooks/useMobile";

export const WatchlistPage = (): ReactElement => {
  const { isMobile } = useMobile();
  return (
    <Box padding={isMobile ? "10px" : "20px"}>
      <Typography variant="h5">Watchlist</Typography>
      <SeriesWatchlist />
    </Box>
  );
};
