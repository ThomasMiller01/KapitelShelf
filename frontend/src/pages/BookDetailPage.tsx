import { Box } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { useParams } from "react-router-dom";

import LoadingCard from "../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../components/base/feedback/RequestErrorCard";
import ItemAppBar from "../components/base/ItemAppBar";
import { seriesApi } from "../lib/api/KapitelShelf.Api";

const BookDetailPage = (): ReactElement => {
  const { seriesId, bookId } = useParams<{
    seriesId: string;
    bookId: string;
  }>();

  const {
    data: book,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["book-by-id"],
    queryFn: async () => {
      if (seriesId === undefined || bookId === undefined) {
        return null;
      }

      const { data } = await seriesApi.seriesSeriesIdBooksBookIdGet(
        seriesId,
        bookId
      );
      return data;
    },
  });

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Book" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard onRetry={refetch} />;
  }

  return (
    <Box>
      <ItemAppBar title={book?.title} />
    </Box>
  );
};

export default BookDetailPage;
