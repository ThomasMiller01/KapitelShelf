import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import { Box, IconButton } from "@mui/material";
import { type ReactElement, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";

import DeleteDialog from "../../components/base/feedback/DeleteDialog";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import ItemAppBar from "../../components/base/ItemAppBar";
import BookDetails from "../../features/book/BookDetails";
import { useBookById } from "../../lib/requests/books/useBookById";
import { useDeleteBook } from "../../lib/requests/books/useDeleteBook";

const BookDetailPage = (): ReactElement => {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const navigate = useNavigate();

  const { data: book, isLoading, isError, refetch } = useBookById(bookId);

  const { mutateAsync: deleteBook } = useDeleteBook(bookId);

  const [deleteOpen, setDeleteOpen] = useState(false);
  const onDelete = async (): Promise<void> => {
    setDeleteOpen(false);

    await deleteBook();

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
