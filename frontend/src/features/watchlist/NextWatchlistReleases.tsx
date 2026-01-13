import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { ScrollableList } from "../../components/base/ScrollableList";
import { ResultCard } from "../../components/watchlist/ResultCard";
import { useApi } from "../../contexts/ApiProvider";
import { useUserProfile } from "../../hooks/useUserProfile";
import { SplitByReleaseWindow } from "../../utils/WatchlistUtils";

const NextWatchlistReleasesList = (): ReactElement => {
  const { clients } = useApi();
  const { profile } = useUserProfile();

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
    return <LoadingCard delayed itemName="Next Releases" small />;
  }

  if (isError) {
    return <RequestErrorCard itemName="Next Releases" onRetry={refetch} />;
  }

  if (data === undefined || data === null || data.length === 0) {
    return <NoItemsFoundCard itemName="Next Releases" small />;
  }

  const { arrivingSoon } = SplitByReleaseWindow(data);
  const flattened = arrivingSoon.flatMap((x) => x.items ?? []);

  return (
    <ScrollableList
      title="Next Releases"
      itemWidth={flattened.length === 0 ? 415 : 150}
      itemGap={16}
    >
      {flattened?.map((x) => (
        <ResultCard key={x.id} book={x} />
      ))}
      {flattened.length == 0 && (
        <NoItemsFoundCard itemName="Next Releases" extraSmall />
      )}
    </ScrollableList>
  );
};

export default NextWatchlistReleasesList;
