import CloseIcon from "@mui/icons-material/Close";
import EditIcon from "@mui/icons-material/Edit";
import { Box, Button, Chip, styled } from "@mui/material";
import { useMutation, useQuery } from "@tanstack/react-query";
import { type ReactElement } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import ItemAppBar from "../../components/base/ItemAppBar";
import { useApi } from "../../contexts/ApiProvider";
import EditableSeriesDetails from "../../features/series/EditableSeriesDetails";
import { useMobile } from "../../hooks/useMobile";
import type { SeriesDTO } from "../../lib/api/KapitelShelf.Api/api";

const EditingBadge = styled(Chip, {
  shouldForwardProp: (prop) => prop !== "isMobile",
})<{ isMobile: boolean }>(({ isMobile }) => ({
  fontSize: isMobile ? "0.82rem" : "0.95rem",
}));

const EditSeriesDetailPage = (): ReactElement => {
  const { seriesId } = useParams<{
    seriesId: string;
  }>();
  const navigate = useNavigate();
  const { clients } = useApi();
  const { isMobile } = useMobile();

  const {
    data: series,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ["series-by-id", seriesId],
    queryFn: async () => {
      if (seriesId === undefined) {
        return null;
      }

      const { data } = await clients.series.seriesSeriesIdGet(seriesId);
      return data;
    },
  });

  const { mutateAsync: mutateUpdateSeries } = useMutation({
    mutationKey: ["update-series-by-id"],
    mutationFn: async (series: SeriesDTO) => {
      if (seriesId === undefined) {
        return null;
      }

      await clients.series.seriesSeriesIdPut(seriesId, series);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Updating series",
        showLoading: true,
        showSuccess: true,
      },
    },
  });

  if (isLoading) {
    return (
      <LoadingCard useLogo delayed itemName="Series to edit" showRandomFacts />
    );
  }

  if (isError || series === undefined || series === null) {
    return <RequestErrorCard itemName="series to edit" onRetry={refetch} />;
  }

  const onUpdate = async (series: SeriesDTO): Promise<void> => {
    await mutateUpdateSeries(series);

    navigate(`/library/series/${seriesId}`);
  };

  return (
    <Box>
      <ItemAppBar
        title={`${series.name}`}
        backTooltip="Go back to series"
        backUrl={`/library/series/${series.id}`}
        addons={[
          <EditingBadge
            key="editing"
            label="EDIT ~ SERIES"
            isMobile={isMobile}
          />,
        ]}
        actions={[
          <Button
            component={Link}
            to={`/library/series/${series.id}`}
            key="cancel"
            startIcon={<CloseIcon />}
            variant="contained"
            size="small"
          >
            Cancel
          </Button>,
        ]}
      />
      <EditableSeriesDetails
        initial={series}
        action={{
          name: "Edit Series",
          onClick: onUpdate,
          icon: <EditIcon />,
        }}
      />
    </Box>
  );
};

export default EditSeriesDetailPage;
