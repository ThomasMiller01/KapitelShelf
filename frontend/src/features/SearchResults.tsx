import { Grid } from "@mui/material";
import { useInfiniteQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import InfiniteScroll from "react-infinite-scroll-component";
import { useNavigate } from "react-router-dom";

import LoadingCard from "../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../components/base/feedback/RequestErrorCard";
import BookCard from "../components/BookCard";
import { searchApi } from "../lib/api/KapitelShelf.Api";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";

const PAGE_SIZE = 24;

interface SearchResultsProps {
  searchterm: string;
}

const SearchResults = ({ searchterm }: SearchResultsProps): ReactElement => {
  const navigate = useNavigate();

  const { data, fetchNextPage, hasNextPage, isLoading, isError, refetch } =
    useInfiniteQuery({
      queryKey: ["search-results", searchterm],
      queryFn: async ({ pageParam = 1 }) => {
        const { data } = await searchApi.searchGet(
          searchterm,
          pageParam,
          PAGE_SIZE
        );
        return data;
      },
      initialPageParam: 1,
      getNextPageParam: (lastPage, pages) => {
        const total = lastPage?.totalCount ?? 0;
        const loaded = pages.flatMap((p) => p?.items).length;
        return loaded < total ? pages.length + 1 : undefined;
      },
    });

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
