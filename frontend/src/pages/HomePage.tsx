import { Box } from "@mui/material";
import type { ReactElement } from "react";

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

  return <Box sx={{ margin: "10px" }}></Box>;
};

export default HomePage;
