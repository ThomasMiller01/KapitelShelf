import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import { Box, IconButton } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { useParams } from "react-router-dom";

import LoadingCard from "../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../components/base/feedback/RequestErrorCard";
import ItemAppBar from "../components/base/ItemAppBar";
import BookDetails from "../features/BookDetails";
import { useNotImplemented } from "../hooks/useNotImplemented";
import { booksApi } from "../lib/api/KapitelShelf.Api";

const BookDetailPage = (): ReactElement => {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const trigger = useNotImplemented();

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
        actions={[
          // eslint-disable-next-line no-magic-numbers
          <IconButton onClick={() => trigger(61)} key="edit">
            <EditIcon />
          </IconButton>,
          // eslint-disable-next-line no-magic-numbers
          <IconButton onClick={() => trigger(62)} key="delete">
            <DeleteIcon />
          </IconButton>,
        ]}
      />
      <BookDetails book={book} />
    </Box>
  );
};

export default BookDetailPage;
