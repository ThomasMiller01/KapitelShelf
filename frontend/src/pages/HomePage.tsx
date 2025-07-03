import { Box } from "@mui/material";
import type { ReactElement } from "react";

import LastVisitedBooksList from "../features/book/LastVisitedBooksList";
import WelcomeScreen from "../features/WelcomeScreen";
import { useWelcomeScreen } from "../hooks/useWelcomeScreen";

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
      <LastVisitedBooksList />
    </Box>
  );
};

export default HomePage;
