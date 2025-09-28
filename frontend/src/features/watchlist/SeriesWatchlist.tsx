import { Divider, Grid, Paper, Stack, Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ReactNode } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { SeriesWatchlistDetails } from "../../components/watchlist/SeriesWatchlistDetails";
import { useApi } from "../../contexts/ApiProvider";
import { useUserProfile } from "../../hooks/useUserProfile";

const SeriesWatchlist: React.FC = () => {
  const { profile } = useUserProfile();
  const { clients } = useApi();

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ["series-watchlist", profile?.id],
    queryFn: async () => {
      if (profile?.id === undefined) {
        return null;
      }

      const { data } = await clients.series.seriesWatchlistGet(profile?.id);
      return data;
    },
  });

  if (isLoading) {
    return (
      <SeriesWatchlistWrapper>
        <LoadingCard delayed itemName="Series Watchlist" small />
      </SeriesWatchlistWrapper>
    );
  }

  if (isError) {
    return (
      <SeriesWatchlistWrapper>
        <RequestErrorCard itemName="Series Watchlist" onRetry={refetch} />
      </SeriesWatchlistWrapper>
    );
  }

  if (data === undefined || data === null || data.length === 0) {
    return (
      <SeriesWatchlistWrapper>
        <NoItemsFoundCard itemName="Series on your Watchlist" extraSmall />
      </SeriesWatchlistWrapper>
    );
  }

  return (
    <SeriesWatchlistWrapper>
      <Grid container spacing={4}>
        {data.map((watchlist) => (
          <Grid key={watchlist.id}>
            <SeriesWatchlistDetails watchlist={watchlist} />
          </Grid>
        ))}
      </Grid>
    </SeriesWatchlistWrapper>
  );
};

interface SeriesWatchlistWrapperProps {
  children: ReactNode;
}

const SeriesWatchlistWrapper: React.FC<SeriesWatchlistWrapperProps> = ({
  children,
}) => (
  <Paper sx={{ my: 2, py: 1.2, px: 2, pb: 2 }}>
    <Stack direction="row" spacing={1} alignItems="baseline" mb="5px">
      <Typography variant="h6">Series</Typography>
      <Typography variant="body2" color="text.secondary">
        under watch
      </Typography>
    </Stack>
    <Divider sx={{ mb: 2 }} />
    {children}
  </Paper>
);

export default SeriesWatchlist;
