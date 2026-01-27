import CloseIcon from "@mui/icons-material/Close";
import EditIcon from "@mui/icons-material/Edit";
import { Box, Button, Chip, styled } from "@mui/material";
import { type ReactElement } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import ItemAppBar from "../../components/base/ItemAppBar";
import EditableBookDetails from "../../features/book/EditableBookDetails";
import { useMobile } from "../../hooks/useMobile";
import type { BookDTO } from "../../lib/api/KapitelShelf.Api/api";
import { useBookById } from "../../lib/requests/books/useBookById";
import { useUpdateBook } from "../../lib/requests/books/useUpdateBook";
import { useUploadCover } from "../../lib/requests/books/useUploadCover";
import { useUploadFile } from "../../lib/requests/books/useUploadFile";

const EditingBadge = styled(Chip, {
  shouldForwardProp: (prop) => prop !== "isMobile",
})<{ isMobile: boolean }>(({ isMobile }) => ({
  fontSize: isMobile ? "0.82rem" : "0.95rem",
}));

const EditBookDetailPage = (): ReactElement => {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const { isMobile } = useMobile();
  const navigate = useNavigate();

  const { data: book, isLoading, isError, refetch } = useBookById(bookId);
  const { mutateAsync: updateBook } = useUpdateBook();
  const { mutateAsync: uploadCover } = useUploadCover();
  const { mutateAsync: uploadFile } = useUploadFile();

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
    bookFile?: File,
  ): Promise<void> => {
    await updateBook({ id: bookId, ...book });

    if (cover !== undefined && bookId !== undefined) {
      try {
        await uploadCover({ bookId, coverFile: cover });
      } catch {
        /* empty */
      }
    }

    if (bookFile !== undefined && bookId !== undefined) {
      try {
        await uploadFile({ bookId, bookFile });
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
