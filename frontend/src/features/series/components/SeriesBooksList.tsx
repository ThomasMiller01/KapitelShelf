import { Grid } from "@mui/material";
import type { ReactElement } from "react";
import InfiniteScroll from "react-infinite-scroll-component";
import { useNavigate } from "react-router-dom";

import LoadingCard from "../../../shared/components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../../shared/components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../../shared/components/base/feedback/RequestErrorCard";
import BookCard from "../../../shared/components/BookCard";
import type { BookDTO } from "../../../lib/api/KapitelShelf.Api/api";
import { useSeriesBookList } from "../hooks/api/useSeriesBookList";

interface SeriesBooksListProps {
  seriesId: string;
}

const SeriesBooksList = ({ seriesId }: SeriesBooksListProps): ReactElement => {
  const navigate = useNavigate();

  const { data, fetchNextPage, hasNextPage, isLoading, isError, refetch } =
    useSeriesBookList(seriesId);

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Books" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard itemName="books" onRetry={refetch} />;
  }

  if (data?.pages[0].totalCount === 0) {
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
      dataLength={data?.pages.flatMap((p) => p.items).length ?? 0}
      next={fetchNextPage}
      hasMore={hasNextPage}
      loader={<LoadingCard itemName="more" />}
      style={{ padding: "24px" }}
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

export default SeriesBooksList;
