import { Box, Typography } from "@mui/material";
import { type ReactElement } from "react";

import SeriesWatchlist from "../features/watchlist/SeriesWatchlist";

export const WatchlistPage = (): ReactElement => (
  <Box padding="20px">
    <Typography variant="h5">Watchlist</Typography>
    <SeriesWatchlist />
  </Box>
);
