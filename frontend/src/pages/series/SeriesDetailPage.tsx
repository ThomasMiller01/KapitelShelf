import AddLinkIcon from "@mui/icons-material/AddLink";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import VisibilityIcon from "@mui/icons-material/Visibility";
import VisibilityOffIcon from "@mui/icons-material/VisibilityOff";
import {
  Box,
  Chip,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  styled,
} from "@mui/material";
import React, { type ReactElement, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";

import DeleteDialog from "../../components/base/feedback/DeleteDialog";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { IconButtonWithTooltip } from "../../components/base/IconButtonWithTooltip";
import ItemAppBar from "../../components/base/ItemAppBar";
import MergeSeriesDialog from "../../features/series/MergeSeriesDialog";
import SeriesBooksList from "../../features/series/SeriesBooksList";
import { useMobile } from "../../hooks/useMobile";
import { useNotification } from "../../hooks/useNotification";
import type { SeriesDTO } from "../../lib/api/KapitelShelf.Api/api";
import { useDeleteSeries } from "../../lib/requests/series/useDeleteSeries";
import { useMergeSeries } from "../../lib/requests/series/useMergeSeries";
import { useSeriesById } from "../../lib/requests/series/useSeriesById";
import { useAddSeriesToWatchlist } from "../../lib/requests/watchlist/useAddSeriesToWatchlist";
import { useRemoveSeriesFromWatchlist } from "../../lib/requests/watchlist/useRemoveSeriesFromWatchlist";
import { useSeriesOnWatchlist } from "../../lib/requests/watchlist/useSeriesOnWatchlist";
import { SeriesSupportsWatchlist } from "../../utils/WatchlistUtils";

const VolumesBadge = styled(Chip, {
  shouldForwardProp: (prop) => prop !== "isMobile",
})<{ isMobile: boolean }>(({ isMobile }) => ({
  fontSize: isMobile ? "0.82rem" : "0.95rem",
}));

const SeriesDetailPage = (): ReactElement => {
  const { seriesId } = useParams<{ seriesId: string }>();

  const navigate = useNavigate();
  const { isMobile } = useMobile();
  const { triggerNavigate } = useNotification();

  const { data: series, isLoading, isError, refetch } = useSeriesById(seriesId);
  const { data: isOnWatchlist } = useSeriesOnWatchlist(series);
  const { mutateAsync: deleteSeries } = useDeleteSeries(seriesId);
  const { mutateAsync: mergeSeries } = useMergeSeries(seriesId);
  const { mutateAsync: addSeriesToWatchlist } = useAddSeriesToWatchlist(
    seriesId,
    () =>
      triggerNavigate({
        operation: "Added series to watchlist",
        itemName: series?.name ?? "Series",
        url: "/watchlist",
      })
  );
  const { mutateAsync: removeSeriesFromWatchlist } =
    useRemoveSeriesFromWatchlist(seriesId);

  const [deleteOpen, setDeleteOpen] = useState(false);
  const onDelete = async (): Promise<void> => {
    setDeleteOpen(false);

    await deleteSeries();

    navigate("/library");
  };

  const [mergeSeriesOpen, setMergeSeriesOpen] = useState(false);
  const onMergeSeries = (series: SeriesDTO): void => {
    setMergeSeriesOpen(false);

    mergeSeries(series.id).then((_) =>
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
          SeriesSupportsWatchlist(series) ? (
            <IconButtonWithTooltip
              tooltip={`${isOnWatchlist ? "Remove" : "Add"} series ${
                isOnWatchlist ? "from" : "to"
              } the watchlist`}
              onClick={
                isOnWatchlist
                  ? () => removeSeriesFromWatchlist()
                  : () => addSeriesToWatchlist()
              }
              key="watchlist"
            >
              {isOnWatchlist ? <VisibilityOffIcon /> : <VisibilityIcon />}
            </IconButtonWithTooltip>
          ) : (
            <React.Fragment key="no-watchlist" />
          ),
          <IconButtonWithTooltip
            tooltip="Edit series"
            component={Link}
            to={`/library/series/${series?.id}/edit`}
            key="edit"
          >
            <EditIcon />
          </IconButtonWithTooltip>,
          <IconButtonWithTooltip
            tooltip="Delete series"
            onClick={() => setDeleteOpen(true)}
            key="delete"
          >
            <DeleteIcon />
          </IconButtonWithTooltip>,
          <OptionsMenu
            key="options"
            onMergeSeriesClick={() => setMergeSeriesOpen(true)}
          />,
        ]}
      />
      <SeriesBooksList seriesId={series?.id ?? ""} />
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
      <IconButtonWithTooltip tooltip="More options" onClick={handleClick}>
        <MoreVertIcon />
      </IconButtonWithTooltip>
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
