import NoteAddIcon from "@mui/icons-material/NoteAdd";
import { Box } from "@mui/material";
import type { ReactElement } from "react";
import { useNavigate } from "react-router-dom";

import FancyText from "../../components/FancyText";
import EditableBookDetails from "../../features/book/EditableBookDetails";
import type { BookDTO } from "../../lib/api/KapitelShelf.Api/api";
import { useCreateBook } from "../../lib/requests/books/useCreateBook";
import { useUploadCover } from "../../lib/requests/books/useUploadCover";
import { useUploadFile } from "../../lib/requests/books/useUploadFile";

const CreateBookPage = (): ReactElement => {
  const navigate = useNavigate();

  const { mutateAsync: createBook } = useCreateBook();
  const { mutateAsync: uploadCover } = useUploadCover();
  const { mutateAsync: uploadFile } = useUploadFile();

  const onCreate = async (
    book: BookDTO,
    cover: File,
    bookFile?: File
  ): Promise<void> => {
    const createdBook = await createBook(book);
    if (
      createdBook?.id === undefined ||
      createdBook.title === undefined ||
      createdBook.title === null
    ) {
      // only continue, if creation was successful
      return;
    }

    // dont upload the nocover image
    if (cover.name !== "nocover.png") {
      await uploadCover({ bookId: createdBook.id, coverFile: cover });
    }

    if (bookFile !== undefined) {
      await uploadFile({ bookId: createdBook.id, bookFile });
    }

    navigate(`/library/books/${createdBook.id}`);
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
