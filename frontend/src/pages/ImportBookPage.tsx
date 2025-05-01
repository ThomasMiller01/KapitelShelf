import { Box, Typography } from "@mui/material";
import type { ReactElement } from "react";

import FancyText from "../components/FancyText";

const ImportBookPage = (): ReactElement => (
  <Box padding="24px">
    <FancyText variant="h4" textAlign="center">
      Import a new Book
    </FancyText>
    <Typography
      variant="overline"
      textAlign="center"
      width="100%"
      fontSize="1rem"
      display="inline-block"
    >
      Not Implemented
    </Typography>
  </Box>
);

export default ImportBookPage;
