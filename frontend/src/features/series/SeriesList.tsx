import { Grid } from "@mui/material";
import type { ReactElement } from "react";
import InfiniteScroll from "react-infinite-scroll-component";
import { useNavigate } from "react-router-dom";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import SeriesCard from "../../components/SeriesCard";
import type { SeriesDTO } from "../../lib/api/KapitelShelf.Api/api";
import { useSeriesList } from "../../lib/requests/series/useSeriesList";

const SeriesList = (): ReactElement => {
  const navigate = useNavigate();

  const { data, fetchNextPage, hasNextPage, isLoading, isError, refetch } =
    useSeriesList();

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Series" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard itemName="series" onRetry={refetch} />;
  }

  if (data?.pages[0].totalCount === 0) {
    return (
      <NoItemsFoundCard
        itemName="Series"
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

export default SeriesList;
