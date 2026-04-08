import { Box } from "@mui/material";
import type { ReactElement } from "react";

import { LastVisitedBooksList } from "../features/book";
import { WelcomeScreen, useWelcomeScreen } from "../features/home";
import { NextWatchlistReleases } from "../features/watchlist";

const HomePage = (): ReactElement => {
  const [isWelcomeShown] = useWelcomeScreen();

  if (!isWelcomeShown) {
    return (
      <Box
        sx={{
          mt: "12%",
          display: "flex",
        }}
      >
        <WelcomeScreen />
      </Box>
    );
  }

  return (
    <Box sx={{ margin: "10px" }}>
      <NextWatchlistReleases />
      <LastVisitedBooksList />
    </Box>
  );
};

export default HomePage;
