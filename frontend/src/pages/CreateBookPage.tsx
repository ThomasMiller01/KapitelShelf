import NoteAddIcon from "@mui/icons-material/NoteAdd";
import { Box } from "@mui/material";
import type { ReactElement } from "react";

import FancyText from "../components/FancyText";
import EditableBookDetails from "../features/EditableBookDetails";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";

const CreateBookPage = (): ReactElement => {
  const onCreate = (book: BookDTO, cover: File): void => {
    console.log("book", book);
    console.log("cover", cover);
  };

  return (
    <Box padding="24px">
      <FancyText variant="h4" textAlign="center" gutterBottom>
        Create a new Book
      </FancyText>
      <EditableBookDetails
        action={{
          name: "Create Book",
          icon: <NoteAddIcon />,
          onClick: onCreate,
        }}
      />
    </Box>
  );
};

export default CreateBookPage;
