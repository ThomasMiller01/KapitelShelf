import { Box } from "@mui/material";
import type { ReactElement } from "react";

import FancyText from "../components/FancyText";
import EditableBookDetails from "../features/EditableBookDetails";

const CreateBookPage = (): ReactElement => (
  <Box padding="24px">
    <FancyText variant="h4" textAlign="center" gutterBottom>
      Create a new Book
    </FancyText>
    <EditableBookDetails />
  </Box>
);

export default CreateBookPage;
