import { Grid } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";

import LoadingCard from "../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../components/base/feedback/RequestErrorCard";
import SeriesCard from "../components/SeriesCard";
import { seriesApi } from "../lib/api/KapitelShelf.Api";

const SeriesList = (): ReactElement => {
  const {
    data: series,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["series"],
    queryFn: async () => {
      const { data } = await seriesApi.seriesSummaryGet();
      return data;
    },
  });

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Series" />;
  }
  if (isError) {
    return <RequestErrorCard onRetry={refetch} />;
  }

  return (
    <Grid container spacing={2} columns={{ xs: 2, sm: 3, md: 4, lg: 6, xl: 8 }}>
      {series?.map((serie) => (
        <Grid key={serie.id} size={1}>
          {serie.lastVolume && <SeriesCard serie={serie} />}
        </Grid>
      ))}
    </Grid>
  );
};

export default SeriesList;
