import { CircularProgress, Grid, Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";

import BookCard from "../components/BookCard";
import { booksApi } from "../lib/api/KapitelShelf.Api";

const BooksList = (): ReactElement => {
  const {
    data: books,
    isLoading,
    isError,
  } = useQuery({
    queryKey: ["books"],
    queryFn: async () => {
      const { data } = await booksApi.booksGet(); // adjust method name to your API
      return data;
    },
  });

  if (isLoading) {
    return <CircularProgress />;
  }
  if (isError) {
    return <Typography>Error loading books</Typography>;
  }

  return (
    <Grid container spacing={2} columns={{ xs: 1, sm: 3, md: 4, lg: 6, xl: 8 }}>
      {books?.map((book) => (
        <Grid key={book.id} size={1}>
          <BookCard book={book} />
        </Grid>
      ))}
    </Grid>
  );
};

export default BooksList;
