import NoteAddIcon from "@mui/icons-material/NoteAdd";
import { Box } from "@mui/material";
import { useMutation } from "@tanstack/react-query";
import type { ReactElement } from "react";

import FancyText from "../components/FancyText";
import EditableBookDetails from "../features/EditableBookDetails";
import { useNotification } from "../hooks/useNotification";
import { booksApi } from "../lib/api/KapitelShelf.Api";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";

interface UploadCoverMutationProps {
  bookId: string;
  coverFile: File;
}

const CreateBookPage = (): ReactElement => {
  const { triggerNavigate } = useNotification();

  const { mutateAsync: mutateCreateBook } = useMutation({
    mutationKey: ["create-book"],
    mutationFn: async (book: BookDTO) => {
      const { data } = await booksApi.booksPost(book);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Creating Book",
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
        operation: "Uploading Cover",
      },
    },
  });

  const onCreate = async (book: BookDTO, cover: File): Promise<void> => {
    const createdBook = await mutateCreateBook(book);
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
      await mutateUploadCover({ bookId: createdBook.id, coverFile: cover });
    }

    triggerNavigate({
      operation: "Created the following book",
      itemName: createdBook.title,
      url: `/library/books/${createdBook.id}`,
    });
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
