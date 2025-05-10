import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import { Box, IconButton } from "@mui/material";
import { useMutation, useQuery } from "@tanstack/react-query";
import { type ReactElement, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";

import DeleteDialog from "../../components/base/feedback/DeleteDialog";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import ItemAppBar from "../../components/base/ItemAppBar";
import BookDetails from "../../features/BookDetails";
import { booksApi } from "../../lib/api/KapitelShelf.Api";

const BookDetailPage = (): ReactElement => {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const navigate = useNavigate();

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

  const { mutateAsync: mutateDeleteBook } = useMutation({
    mutationKey: ["delete-book", bookId],
    mutationFn: async () => {
      if (bookId === undefined) {
        return null;
      }

      await booksApi.booksBookIdDelete(bookId);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Deleting book",
        showLoading: true,
        showSuccess: true,
      },
    },
  });

  const [deleteOpen, setDeleteOpen] = useState(false);
  const onDelete = async (): Promise<void> => {
    setDeleteOpen(false);

    await mutateDeleteBook();

    navigate(`/library/series/${book?.series?.id}`);
  };

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Book" showRandomFacts />;
  }

  if (isError || book === undefined || book === null) {
    return <RequestErrorCard itemName="book" onRetry={refetch} />;
  }

  return (
    <Box>
      <ItemAppBar
        title={book?.title}
        backTooltip="Go to series"
        backUrl={`/library/series/${book.series?.id}`}
        actions={[
          <IconButton
            component={Link}
            to={`/library/books/${book.id}/edit`}
            key="edit"
          >
            <EditIcon />
          </IconButton>,
          <IconButton onClick={() => setDeleteOpen(true)} key="delete">
            <DeleteIcon />
          </IconButton>,
        ]}
      />
      <BookDetails book={book} />
      <DeleteDialog
        open={deleteOpen}
        onCancel={() => setDeleteOpen(false)}
        onConfirm={onDelete}
        title="Confirm to delete this book"
        description="Are you sure you want to delete this book? This action cannot be undone."
      />
    </Box>
  );
};

export default BookDetailPage;
