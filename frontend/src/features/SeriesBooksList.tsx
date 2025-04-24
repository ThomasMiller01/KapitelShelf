import { Grid } from "@mui/material";
import { useInfiniteQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import InfiniteScroll from "react-infinite-scroll-component";

import LoadingCard from "../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../components/base/feedback/RequestErrorCard";
import BookCard from "../components/BookCard";
import { seriesApi } from "../lib/api/KapitelShelf.Api";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";

const PAGE_SIZE = 24;

interface SeriesBooksListProps {
  seriesId: string;
}

const SeriesBooksList = ({ seriesId }: SeriesBooksListProps): ReactElement => {
  const { data, fetchNextPage, hasNextPage, isLoading, isError, refetch } =
    useInfiniteQuery({
      queryKey: ["series"],
      queryFn: async ({ pageParam = 1 }) => {
        const { data } = await seriesApi.seriesSeriesIdBooksGet(
          seriesId,
          pageParam,
          PAGE_SIZE
        );
        return data;
      },
      initialPageParam: 1,
      getNextPageParam: (lastPage, pages) => {
        const total = lastPage.totalCount ?? 0;
        const loaded = pages.flatMap((p) => p.items).length;
        return loaded < total ? pages.length + 1 : undefined;
      },
    });

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Series" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard onRetry={refetch} />;
  }

  return (
    <InfiniteScroll
      dataLength={data?.pages.flatMap((p) => p.items).length ?? 0}
      next={fetchNextPage}
      hasMore={hasNextPage}
      loader={<LoadingCard itemName="more" />}
    >
      <Grid
        container
        spacing={2}
        columns={{ xs: 2, sm: 3, md: 4, lg: 6, xl: 8 }}
      >
        {data?.pages
          .flatMap((p) => p.items)
          .filter((book): book is BookDTO => Boolean(book))
          .map((book) => (
            <Grid key={book.id} size={1}>
              <BookCard book={book} />
            </Grid>
          ))}
      </Grid>
    </InfiniteScroll>
  );
};

export default SeriesBooksList;
