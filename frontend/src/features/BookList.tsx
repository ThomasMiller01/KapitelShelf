import { Grid } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";

import LoadingCard from "../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../components/base/feedback/RequestErrorCard";
import BookCard from "../components/BookCard";
import { booksApi } from "../lib/api/KapitelShelf.Api";

const BooksList = (): ReactElement => {
  const {
    data: books,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["books"],
    queryFn: async () => {
      const { data } = await booksApi.booksGet();
      return data;
    },
  });

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Books" />;
  }
  if (isError) {
    return <RequestErrorCard onRetry={refetch} />;
  }

  return (
    <Grid container spacing={2} columns={{ xs: 2, sm: 3, md: 4, lg: 6, xl: 8 }}>
      {books?.map((book) => (
        <Grid key={book.id} size={1}>
          <BookCard book={book} showAuthor />
        </Grid>
      ))}
    </Grid>
  );
};

export default BooksList;
