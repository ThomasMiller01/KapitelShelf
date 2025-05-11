import CloseIcon from "@mui/icons-material/Close";
import EditIcon from "@mui/icons-material/Edit";
import { Box, Button, Chip, styled } from "@mui/material";
import { useMutation, useQuery } from "@tanstack/react-query";
import { type ReactElement } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import ItemAppBar from "../../components/base/ItemAppBar";
import EditableBookDetails from "../../features/book/EditableBookDetails";
import { useMobile } from "../../hooks/useMobile";
import { booksApi } from "../../lib/api/KapitelShelf.Api";
import type { BookDTO } from "../../lib/api/KapitelShelf.Api/api";

interface UploadCoverMutationProps {
  bookId: string;
  coverFile: File;
}

interface UploadFileMutationProps {
  bookId: string;
  bookFile: File;
}

const EditingBadge = styled(Chip, {
  shouldForwardProp: (prop) => prop !== "isMobile",
})<{ isMobile: boolean }>(({ isMobile }) => ({
  fontSize: isMobile ? "0.82rem" : "0.95rem",
}));

const EditBookDetailPage = (): ReactElement => {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const navigate = useNavigate();
  const { isMobile } = useMobile();

  const {
    data: book,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["book-by-id", bookId],
    queryFn: async () => {
      if (bookId === undefined) {
        return null;
      }

      const { data } = await booksApi.booksBookIdGet(bookId);
      return data;
    },
  });

  const { mutateAsync: mutateUpdateBook } = useMutation({
    mutationKey: ["update-book-by-id"],
    mutationFn: async (book: BookDTO) => {
      if (bookId === undefined) {
        return null;
      }

      await booksApi.booksBookIdPut(bookId, book);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Updating book",
        showLoading: true,
        showSuccess: true,
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

  const { mutateAsync: mutateUploadFile } = useMutation({
    mutationKey: ["upload-file"],
    mutationFn: async ({ bookId, bookFile }: UploadFileMutationProps) => {
      const { data } = await booksApi.booksBookIdFilePost(bookId, bookFile);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Uploading file",
        showLoading: true,
      },
    },
  });

  if (isLoading) {
    return (
      <LoadingCard useLogo delayed itemName="Book to edit" showRandomFacts />
    );
  }

  if (isError || book === undefined || book === null) {
    return <RequestErrorCard itemName="book to edit" onRetry={refetch} />;
  }

  const onUpdate = async (
    book: BookDTO,
    cover: File,
    bookFile?: File
  ): Promise<void> => {
    await mutateUpdateBook(book);

    if (cover !== undefined && bookId !== undefined) {
      try {
        await mutateUploadCover({ bookId, coverFile: cover });
      } catch {
        /* empty */
      }
    }

    if (bookFile !== undefined && bookId !== undefined) {
      try {
        await mutateUploadFile({ bookId, bookFile });
      } catch {
        /* empty */
      }
    }

    navigate(`/library/books/${bookId}`);
  };

  return (
    <Box>
      <ItemAppBar
        title={`${book?.title}`}
        backTooltip="Go back to book"
        backUrl={`/library/books/${book.id}`}
        addons={[
          <EditingBadge
            key="editing"
            label="EDIT ~ BOOK"
            isMobile={isMobile}
          />,
        ]}
        actions={[
          <Button
            component={Link}
            to={`/library/books/${book.id}`}
            key="cancel"
            startIcon={<CloseIcon />}
            variant="contained"
            size="small"
          >
            Cancel
          </Button>,
        ]}
      />
      <EditableBookDetails
        initial={book}
        action={{
          name: "Edit Book",
          onClick: onUpdate,
          icon: <EditIcon />,
        }}
      />
    </Box>
  );
};

export default EditBookDetailPage;
