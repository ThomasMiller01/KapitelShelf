import type { ReactElement } from "react";

import LoadingCard from "../../../shared/components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../../shared/components/base/feedback/RequestErrorCard";
import { ScrollableList } from "../../../shared/components/base/ScrollableList";
import { useWatchlist } from "../hooks/api/useWatchlist";
import { SplitByReleaseWindow } from "../utils/WatchlistUtils";
import { ResultCard } from "./ResultCard";

const NextWatchlistReleasesList = (): ReactElement => {
  const { data, isLoading, isError, refetch } = useWatchlist();

  if (isLoading) {
    return <LoadingCard delayed itemName="Next Releases" small />;
  }

  if (isError) {
    return <RequestErrorCard itemName="Next Releases" onRetry={refetch} />;
  }

  if (data === undefined || data === null || data.length === 0) {
    return <></>;
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
    </ScrollableList>
  );
};

export default NextWatchlistReleasesList;
