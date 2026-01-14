import { Grid } from "@mui/material";
import type { ReactElement } from "react";
import InfiniteScroll from "react-infinite-scroll-component";
import { useNavigate } from "react-router-dom";

import LoadingCard from "../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../components/base/feedback/RequestErrorCard";
import BookCard from "../components/BookCard";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";
import { useBookSearch } from "../lib/requests/books/useBookSearch";

interface SearchResultsProps {
  searchterm: string;
}

const SearchResults = ({ searchterm }: SearchResultsProps): ReactElement => {
  const navigate = useNavigate();

  const { data, fetchNextPage, hasNextPage, isLoading, isError, refetch } =
    useBookSearch(searchterm);

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Books" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard itemName="books" onRetry={refetch} />;
  }

  if (
    data?.pages[0].totalCount === 0 ||
    data?.pages[0].totalCount === undefined
  ) {
    return (
      <NoItemsFoundCard
        itemName="Books"
        useLogo
        onCreate={() => navigate("/library/books/create")}
      />
    );
  }

  return (
    <InfiniteScroll
      dataLength={data.pages.flatMap((p) => p.items).length}
      next={fetchNextPage}
      hasMore={hasNextPage}
      loader={<LoadingCard itemName="more" />}
    >
      <Grid container spacing={2}>
        {data?.pages
          .flatMap((p) => p.items)
          .filter((book): book is BookDTO => Boolean(book))
          .map((book) => (
            <Grid key={book.id}>
              <BookCard book={book} />
            </Grid>
          ))}
      </Grid>
    </InfiniteScroll>
  );
};

export default SearchResults;
