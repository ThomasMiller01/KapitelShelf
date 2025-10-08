import { Box, Divider, Grid, Paper, Stack, Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { WatchlistDetails } from "../../components/watchlist/WatchlistDetails";
import { useApi } from "../../contexts/ApiProvider";
import { useMobile } from "../../hooks/useMobile";
import { useUserProfile } from "../../hooks/useUserProfile";
import type { BookDTO, WatchlistDTO } from "../../lib/api/KapitelShelf.Api";

const Watchlist: React.FC = () => {
  const { profile } = useUserProfile();
  const { clients } = useApi();

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ["watchlist", profile?.id],
    queryFn: async () => {
      if (profile?.id === undefined) {
        return null;
      }

      const { data } = await clients.watchlist.watchlistGet(profile?.id);
      return data;
    },
  });

  if (isLoading) {
    return <LoadingCard delayed itemName="Watchlist" small />;
  }

  if (isError) {
    return <RequestErrorCard itemName="Watchlist" onRetry={refetch} />;
  }

  if (data === undefined || data === null || data.length === 0) {
    return <NoItemsFoundCard itemName="Series on your Watchlist" extraSmall />;
  }

  // get release date from the first item
  const getFirstReleaseDate = (watchlist: WatchlistDTO): Date | null => {
    const first: BookDTO | undefined = watchlist.items?.[0];
    if (!first?.releaseDate) {
      return null;
    }
    const date = new Date(first.releaseDate);
    return isNaN(date.getTime()) ? null : date;
  };

  const now = new Date();
  const weekFromNow = new Date(now);
  weekFromNow.setDate(now.getDate() + 7);
  const monthFromNow = new Date(now);
  monthFromNow.setDate(now.getDate() + 30);

  const arrivingSoon = data.filter((w) => {
    const date = getFirstReleaseDate(w);
    return date && date <= weekFromNow;
  });

  const comingUp = data.filter((w) => {
    const date = getFirstReleaseDate(w);
    return date && date > weekFromNow && date <= monthFromNow;
  });

  const later = data.filter((w) => {
    const date = getFirstReleaseDate(w);
    return !date || date > monthFromNow;
  });

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
