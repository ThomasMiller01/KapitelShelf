import { Grid } from "@mui/material";
import { useInfiniteQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import InfiniteScroll from "react-infinite-scroll-component";
import { useNavigate } from "react-router-dom";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import SeriesCard from "../../components/SeriesCard";
import { useApi } from "../../contexts/ApiProvider";
import type { SeriesDTO } from "../../lib/api/KapitelShelf.Api/api";

const PAGE_SIZE = 24;

const SeriesList = (): ReactElement => {
  const navigate = useNavigate();
  const { clients } = useApi();

  const { data, fetchNextPage, hasNextPage, isLoading, isError, refetch } =
    useInfiniteQuery({
      queryKey: ["series"],
      queryFn: async ({ pageParam = 1 }) => {
        const { data } = await clients.series.seriesGet(pageParam, PAGE_SIZE);
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
