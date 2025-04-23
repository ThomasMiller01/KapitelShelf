import { Box } from "@mui/material";
import type { ReactElement } from "react";

import BooksList from "../features/BookList";

const BooksPage = (): ReactElement => (
  <Box>
    <BooksList />
  </Box>
);

export default BooksPage;
