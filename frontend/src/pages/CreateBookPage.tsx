import { Box } from "@mui/material";
import type { ReactElement } from "react";

import FancyText from "../components/FancyText";

const CreateBookPage = (): ReactElement => (
  <Box padding="24px">
    <FancyText variant="h4" textAlign="center">
      Create a new Book
    </FancyText>
  </Box>
);

export default CreateBookPage;
