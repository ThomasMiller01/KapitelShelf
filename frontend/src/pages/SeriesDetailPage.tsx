import { Box } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { useParams } from "react-router-dom";

import LoadingCard from "../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../components/base/feedback/RequestErrorCard";
import ItemAppBar from "../components/base/ItemAppBar";
import SeriesBooksList from "../features/SeriesBooksList";
import { seriesApi } from "../lib/api/KapitelShelf.Api";

const SeriesDetailPage = (): ReactElement => {
  const { seriesId } = useParams<{ seriesId: string }>();

  const {
    data: series,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["series-by-id"],
    queryFn: async () => {
      if (seriesId === undefined) {
        return null;
      }

      const { data } = await seriesApi.seriesSeriesIdGet(seriesId);
      return data;
    },
  });

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Series" showRandomFacts />;
  }

  if (isError) {
    return <RequestErrorCard onRetry={refetch} />;
  }

  return (
    <Box>
      <ItemAppBar title={series?.name} />
      <Box padding="24px">
        <SeriesBooksList seriesId={series?.id ?? ""} />
      </Box>
    </Box>
  );
};

export default SeriesDetailPage;
