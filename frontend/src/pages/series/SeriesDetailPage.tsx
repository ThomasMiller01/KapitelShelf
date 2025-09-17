import AddLinkIcon from "@mui/icons-material/AddLink";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import {
  Box,
  Chip,
  IconButton,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  styled,
} from "@mui/material";
import { useMutation, useQuery } from "@tanstack/react-query";
import { type ReactElement, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";

import DeleteDialog from "../../components/base/feedback/DeleteDialog";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import ItemAppBar from "../../components/base/ItemAppBar";
import { useApi } from "../../contexts/ApiProvider";
import MergeSeriesDialog from "../../features/series/MergeSeriesDialog";
import SeriesBooksList from "../../features/series/SeriesBooksList";
import { useMobile } from "../../hooks/useMobile";
import type { SeriesDTO } from "../../lib/api/KapitelShelf.Api/api";

const VolumesBadge = styled(Chip, {
  shouldForwardProp: (prop) => prop !== "isMobile",
})<{ isMobile: boolean }>(({ isMobile }) => ({
  fontSize: isMobile ? "0.82rem" : "0.95rem",
}));

const SeriesDetailPage = (): ReactElement => {
  const { seriesId } = useParams<{ seriesId: string }>();
  const navigate = useNavigate();
  const { clients } = useApi();
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

      const { data } = await clients.series.seriesSeriesIdGet(seriesId);
      return data;
    },
  });

  const { mutateAsync: mutateDeleteSeries } = useMutation({
    mutationKey: ["delete-series", seriesId],
    mutationFn: async () => {
      if (seriesId === undefined) {
        return null;
      }

      await clients.series.seriesSeriesIdDelete(seriesId);
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

  const { mutateAsync: mutateMergeSeries } = useMutation({
    mutationKey: ["merge-series", seriesId],
    mutationFn: async (targetSeriesId: string | undefined) => {
      if (seriesId === undefined || targetSeriesId === undefined) {
        return null;
      }

      await clients.series.seriesSeriesIdMergeTargetSeriesIdPut(
        seriesId,
        targetSeriesId
      );
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Merging series",
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

  const [mergeSeriesOpen, setMergeSeriesOpen] = useState(false);
  const onMergeSeries = (series: SeriesDTO): void => {
    setMergeSeriesOpen(false);

    mutateMergeSeries(series.id).then((_) =>
      navigate(`/library/series/${series.id}`)
    );
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
          <IconButton
            component={Link}
            to={`/library/series/${series?.id}/edit`}
            key="edit"
          >
            <EditIcon />
          </IconButton>,
          <IconButton onClick={() => setDeleteOpen(true)} key="delete">
            <DeleteIcon />
          </IconButton>,
          <OptionsMenu
            key="options"
            onMergeSeriesClick={() => setMergeSeriesOpen(true)}
          />,
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
      <MergeSeriesDialog
        series={series}
        open={mergeSeriesOpen}
        onCancel={() => setMergeSeriesOpen(false)}
        onConfirm={onMergeSeries}
      />
    </Box>
  );
};

interface OptionsMenuProps {
  onMergeSeriesClick: () => void;
}

const OptionsMenu = ({
  onMergeSeriesClick,
}: OptionsMenuProps): ReactElement => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClick = (event: React.MouseEvent<HTMLButtonElement>): void => {
    setAnchorEl(event.currentTarget);
  };
  const handleClose = (): void => {
    setAnchorEl(null);
  };

  return (
    <>
      <IconButton onClick={handleClick}>
        <MoreVertIcon />
      </IconButton>
      <Menu anchorEl={anchorEl} open={open} onClose={handleClose}>
        <OptionMenuItem
          text="Merge with Series"
          icon={<AddLinkIcon />}
          onClick={() => {
            onMergeSeriesClick();
            handleClose();
          }}
        />
      </Menu>
    </>
  );
};

interface OptionMenuItemProps {
  text: string;
  icon?: ReactElement;
  onClick: () => void;
}

const OptionMenuItem = ({
  text,
  icon,
  onClick,
}: OptionMenuItemProps): ReactElement => (
  <MenuItem onClick={onClick}>
    {icon && <ListItemIcon>{icon}</ListItemIcon>}
    <ListItemText>{text}</ListItemText>
  </MenuItem>
);

export default SeriesDetailPage;
