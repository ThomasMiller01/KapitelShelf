import type { ReactElement } from "react";

import LoadingCard from "../../../shared/components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../../shared/components/base/feedback/RequestErrorCard";
import { ScrollableList } from "../../../shared/components/base/ScrollableList";
import BookCard from "../../../shared/components/BookCard";
import { useLastReadBooks } from "../hooks/api/useLastReadBooks";

const LastReadBooksList = (): ReactElement => {
  const { data, isLoading, isError, refetch } = useLastReadBooks();

  if (isLoading) {
    return <LoadingCard delayed itemName="Last Read Books" small />;
  }

  if (isError) {
    return <RequestErrorCard itemName="Last Read Books" onRetry={refetch} />;
  }

  if (data === undefined || data === null || data.totalCount === 0) {
    return <></>;
  }

  return (
    <ScrollableList
      title="Last Read Books"
      itemWidth={data?.totalCount == 0 ? 450 : 150}
      itemGap={16}
    >
      {data?.items?.map((x) => (
        <BookCard key={x.id} book={x} small showRating={false} />
      ))}
    </ScrollableList>
  );
};

export default LastReadBooksList;
