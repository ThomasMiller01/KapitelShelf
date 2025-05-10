import DeleteIcon from "@mui/icons-material/Delete";
import { Box, Chip, IconButton, styled } from "@mui/material";
import { useMutation, useQuery } from "@tanstack/react-query";
import { type ReactElement, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import DeleteDialog from "../components/base/feedback/DeleteDialog";
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
  const navigate = useNavigate();
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

  const { mutateAsync: mutateDeleteSeries } = useMutation({
    mutationKey: ["delete-series", series],
    mutationFn: async () => {
      if (seriesId === undefined) {
        return null;
      }

      await seriesApi.seriesSeriesIdDelete(seriesId);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Deleting series",
        showLoading: true,
        showSuccess: true,
      },
    },
  });

  const [deleteOpen, setDeleteOpen] = useState(false);
  const onDelete = async (): Promise<void> => {
    setDeleteOpen(false);

    await mutateDeleteSeries();

    navigate("/library");
  };

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
        actions={[
          <IconButton onClick={() => setDeleteOpen(true)} key="delete">
            <DeleteIcon />
          </IconButton>,
        ]}
      />
      <Box padding="24px">
        <SeriesBooksList seriesId={series?.id ?? ""} />
      </Box>
      <DeleteDialog
        open={deleteOpen}
        onCancel={() => setDeleteOpen(false)}
        onConfirm={onDelete}
        title="Confirm to delete this series"
        description="Are you sure you want to delete this series? This action cannot be undone."
      />
    </Box>
  );
};

export default SeriesDetailPage;
