import { Box, Divider, Grid, Paper, Stack, Typography } from "@mui/material";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { WatchlistDetails } from "../../components/watchlist/WatchlistDetails";
import { useMobile } from "../../hooks/useMobile";
import type { WatchlistDTO } from "../../lib/api/KapitelShelf.Api";
import { useWatchlist } from "../../lib/requests/watchlist/useWatchlist";
import { SplitByReleaseWindow } from "../../utils/WatchlistUtils";

const Watchlist: React.FC = () => {
  const { data, isLoading, isError, refetch } = useWatchlist();

  if (isLoading) {
    return <LoadingCard delayed itemName="Watchlist" small />;
  }

  if (isError) {
    return <RequestErrorCard itemName="Watchlist" onRetry={refetch} />;
  }

  if (data === undefined || data === null || data.length === 0) {
    return <NoItemsFoundCard itemName="Series on your Watchlist" extraSmall />;
  }

  const { arrivingSoon, comingUp, later } = SplitByReleaseWindow(data);

  return (
    <Box>
      <WatchtlistCategory
        label="Arriving Soon"
        description="next 7 days"
        watchlists={arrivingSoon}
      />
      <WatchtlistCategory
        label="Coming Up"
        description="next 30 days"
        watchlists={comingUp}
      />
      <WatchtlistCategory
        label="Further Ahead"
        description="patience required"
        watchlists={later}
      />
    </Box>
  );
};

interface WatchlistReleaseTimeCategoryProps {
  label: string;
  description: string;
  watchlists: WatchlistDTO[];
}

const WatchtlistCategory: React.FC<WatchlistReleaseTimeCategoryProps> = ({
  label,
  description,
  watchlists,
}) => {
  const { isMobile } = useMobile();

  return (
    <Paper sx={{ my: 2, mb: 3, py: 1.5, px: isMobile ? "10px" : 2, pb: 2 }}>
      <Stack direction="row" spacing={1} alignItems="baseline" mb="5px">
        <Typography variant="h6">{label}</Typography>
        <Typography variant="body2" color="text.secondary">
          {description}
        </Typography>
      </Stack>
      <Divider sx={{ mb: 2 }} />
      <Grid container rowSpacing={4} columnSpacing={6}>
        {watchlists.map((watchlist) => (
          <Grid key={watchlist.id}>
            <WatchlistDetails watchlist={watchlist} />
          </Grid>
        ))}
      </Grid>
      {watchlists.length === 0 && (
        <Typography variant="body2" color="text.secondary" mt={-0.5}>
          No releases announced.
        </Typography>
      )}
    </Paper>
  );
};

export default Watchlist;
