import type { ReactElement } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { ScrollableList } from "../../components/base/ScrollableList";
import { ResultCard } from "../../components/watchlist/ResultCard";
import { useWatchlist } from "../../lib/requests/watchlist/useWatchlist";
import { SplitByReleaseWindow } from "../../utils/WatchlistUtils";

const NextWatchlistReleasesList = (): ReactElement => {
  const { data, isLoading, isError, refetch } = useWatchlist();

  if (isLoading) {
    return <LoadingCard delayed itemName="Next Releases" small />;
  }

  if (isError) {
    return <RequestErrorCard itemName="Next Releases" onRetry={refetch} />;
  }

  if (data === undefined || data === null) {
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
