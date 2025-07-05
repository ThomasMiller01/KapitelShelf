import { Box } from "@mui/material";
import type { ReactElement } from "react";

import FancyText from "../../components/FancyText";
import { ProfileList } from "../../features/user/ProfileList";

export const SelectProfilePage = (): ReactElement => (
  <Box
    minHeight="90vh"
    display="flex"
    padding="20px"
    flexDirection="column"
    alignItems="center"
    justifyContent="center"
    bgcolor="background.default"
  >
    <FancyText variant="h4" sx={{ mb: 6 }}>
      Who's reading?
    </FancyText>
    <ProfileList />
  </Box>
);
