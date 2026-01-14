import { Grid } from "@mui/material";
import { useState } from "react";
import InfiniteScroll from "react-infinite-scroll-component";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import SeriesCard from "../../components/SeriesCard";
import { SeriesDTO } from "../../lib/api/KapitelShelf.Api";
import { useSeriesList } from "../../lib/requests/series/useSeriesList";

export const ManageSeriesList = () => {
  const [pageSize, setPageSize] = useState(100);

  const { data, fetchNextPage, hasNextPage, isLoading, isError, refetch } =
    useSeriesList(pageSize);

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Series" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard itemName="series" onRetry={refetch} />;
  }

  return (
    <InfiniteScroll
      dataLength={data?.pages.flatMap((p) => p.items).length ?? 0}
      next={fetchNextPage}
      hasMore={hasNextPage}
      loader={<LoadingCard itemName="more" />}
    >
      <Grid container spacing={2}>
        {data?.pages
          .flatMap((p) => p.items)
          .filter((series): series is SeriesDTO => Boolean(series))
          .map((series) => (
            <Grid key={series.id}>
              <SeriesCard series={series} />
            </Grid>
          ))}
      </Grid>
    </InfiniteScroll>
  );
};
