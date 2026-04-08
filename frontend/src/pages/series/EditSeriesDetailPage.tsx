import CloseIcon from "@mui/icons-material/Close";
import EditIcon from "@mui/icons-material/Edit";
import { Box, Button, Chip, styled } from "@mui/material";
import { type ReactElement } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";

import {
  EditableSeriesDetails,
  useSeriesById,
  useUpdateSeries,
} from "../../features/series";
import LoadingCard from "../../shared/components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../shared/components/base/feedback/RequestErrorCard";
import ItemAppBar from "../../shared/components/base/ItemAppBar";
import { useMobile } from "../../shared/hooks/useMobile";
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
  const { isMobile } = useMobile();
  const navigate = useNavigate();

  const { data: series, isLoading, isError, refetch } = useSeriesById(seriesId);
  const { mutateAsync: updateSeries } = useUpdateSeries();

  if (isLoading) {
    return (
      <LoadingCard useLogo delayed itemName="Series to edit" showRandomFacts />
    );
  }

  if (isError || series === undefined || series === null) {
    return <RequestErrorCard itemName="series to edit" onRetry={refetch} />;
  }

  const onUpdate = async (series: SeriesDTO): Promise<void> => {
    await updateSeries({ id: seriesId, ...series });

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
