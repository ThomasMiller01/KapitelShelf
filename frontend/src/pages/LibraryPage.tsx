import { Box } from "@mui/material";
import type { ReactElement } from "react";

import SeriesList from "../features/series/SeriesList";

const BooksPage = (): ReactElement => (
  <Box padding="24px">
    <SeriesList />
  </Box>
);

export default BooksPage;
