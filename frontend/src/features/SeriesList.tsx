import { Grid, Link, Stack, Typography } from "@mui/material";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useSnackbar } from "notistack";
import type { ReactElement } from "react";
import InfiniteScroll from "react-infinite-scroll-component";

import LoadingCard from "../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../components/base/feedback/RequestErrorCard";
import SeriesCard from "../components/SeriesCard";
import { seriesApi } from "../lib/api/KapitelShelf.Api";
import type { SeriesSummaryDTO } from "../lib/api/KapitelShelf.Api/api";

const PAGE_SIZE = 24;

const SeriesList = (): ReactElement => {
  const { enqueueSnackbar } = useSnackbar();

  const { data, fetchNextPage, hasNextPage, isLoading, isError, refetch } =
    useInfiniteQuery({
      queryKey: ["series"],
      queryFn: async ({ pageParam = 1 }) => {
        const { data } = await seriesApi.seriesSummaryGet(pageParam, PAGE_SIZE);
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

  if (data?.pages[0].totalCount === 0) {
    return (
      <NoItemsFoundCard
        itemName="Series"
        useLogo
        onCreate={() =>
          enqueueSnackbar(
            <Stack direction="row" spacing={0.8} alignItems="center">
              <Typography>Not Implemented</Typography>
              <Link
                href="https://github.com/ThomasMiller01/KapitelShelf/issues/46"
                fontSize="1rem"
                target="_blank"
                rel="noreferrer"
              >
                [Issue #46]
              </Link>
            </Stack>,
            {
              variant: "info",
            }
          )
        }
      />
    );
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
          .filter((serie): serie is SeriesSummaryDTO => Boolean(serie))
          .map((serie) => (
            <Grid key={serie.id} size={1}>
              {serie.lastVolume && <SeriesCard serie={serie} />}
            </Grid>
          ))}
      </Grid>
    </InfiniteScroll>
  );
};

export default SeriesList;
