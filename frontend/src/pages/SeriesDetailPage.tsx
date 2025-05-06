import { Box, Chip, styled } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { type ReactElement } from "react";
import { useParams } from "react-router-dom";

import LoadingCard from "../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../components/base/feedback/RequestErrorCard";
import ItemAppBar from "../components/base/ItemAppBar";
import SeriesBooksList from "../features/SeriesBooksList";
import { useMobile } from "../hooks/useMobile";
import { seriesApi } from "../lib/api/KapitelShelf.Api";

const VolumesBadge = styled(Chip, {
  shouldForwardProp: (prop) => prop !== "isMobile",
})<{ isMobile: boolean }>(({ isMobile }) => ({
  fontSize: isMobile ? "0.82rem" : "0.95rem",
}));

const SeriesDetailPage = (): ReactElement => {
  const { seriesId } = useParams<{ seriesId: string }>();
  const { isMobile } = useMobile();

  const {
    data: series,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["series-details", seriesId],
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
    return <RequestErrorCard itemName="series" onRetry={refetch} />;
  }

  return (
    <Box>
      <ItemAppBar
        title={series?.name}
        backTooltip="Go to library"
        backUrl="/library"
        addons={[
          <VolumesBadge
            key="series-count"
            label={`${series?.totalBooks} Volume(s)`}
            isMobile={isMobile}
          />,
        ]}
      />
      <Box padding="24px">
        <SeriesBooksList seriesId={series?.id ?? ""} />
      </Box>
    </Box>
  );
};

export default SeriesDetailPage;
