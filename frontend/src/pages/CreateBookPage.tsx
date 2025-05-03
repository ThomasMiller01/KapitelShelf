import NoteAddIcon from "@mui/icons-material/NoteAdd";
import { Box } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import type { ReactElement } from "react";

import FancyText from "../components/FancyText";
import EditableBookDetails from "../features/EditableBookDetails";
import { booksApi } from "../lib/api/KapitelShelf.Api";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";

interface UploadCoverMutationProps {
  bookId: string;
  coverFile: File;
}

const CreateBookPage = (): ReactElement => {
  const { mutateAsync: mutateCreateBook } = useMutation({
    mutationKey: ["create-book"],
    mutationFn: async (book: BookDTO) => {
      const { data } = await booksApi.booksPost(book);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Creating book",
      },
    },
  });

  const { mutateAsync: mutateUploadCover } = useMutation({
    mutationKey: ["upload-cover"],
    mutationFn: async ({ bookId, coverFile }: UploadCoverMutationProps) => {
      const { data } = await booksApi.booksBookIdCoverPost(bookId, coverFile);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Uploading cover",
      },
    },
  });

  const onCreate = async (book: BookDTO, cover: File): Promise<void> => {
    const createdBook = await mutateCreateBook(book);
    if (createdBook?.id === undefined) {
      // only continue, if creation was successful
      return;
    }

    await mutateUploadCover({ bookId: createdBook.id, coverFile: cover });
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
