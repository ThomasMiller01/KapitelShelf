import { Grid, Paper } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import type { ReactNode } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { WatchlistDetails } from "../../components/watchlist/WatchlistDetails";
import { useApi } from "../../contexts/ApiProvider";
import { useMobile } from "../../hooks/useMobile";
import { useUserProfile } from "../../hooks/useUserProfile";

const Watchlist: React.FC = () => {
  const { profile } = useUserProfile();
  const { clients } = useApi();

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ["watchlist", profile?.id],
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
      <WatchlistWrapper>
        <LoadingCard delayed itemName="Watchlist" small />
      </WatchlistWrapper>
    );
  }

  if (isError) {
    return (
      <WatchlistWrapper>
        <RequestErrorCard itemName="Watchlist" onRetry={refetch} />
      </WatchlistWrapper>
    );
  }

  if (data === undefined || data === null || data.length === 0) {
    return (
      <WatchlistWrapper>
        <NoItemsFoundCard itemName="Series on your Watchlist" extraSmall />
      </WatchlistWrapper>
    );
  }

  return (
    <WatchlistWrapper>
      <Grid container rowSpacing={3} columnSpacing={6}>
        {data.map((watchlist) => (
          <Grid key={watchlist.id}>
            <WatchlistDetails watchlist={watchlist} />
          </Grid>
        ))}
      </Grid>
    </WatchlistWrapper>
  );
};

interface WatchlistWrapperProps {
  children: ReactNode;
}

const WatchlistWrapper: React.FC<WatchlistWrapperProps> = ({ children }) => {
  const { isMobile } = useMobile();
  return (
    <Paper sx={{ my: 2, py: 1.5, px: isMobile ? "10px" : 2, pb: 2 }}>
      {children}
    </Paper>
  );
};

export default Watchlist;
