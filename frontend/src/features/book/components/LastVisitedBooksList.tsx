import type { ReactElement } from "react";

import LoadingCard from "../../../shared/components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../../shared/components/base/feedback/RequestErrorCard";
import { ScrollableList } from "../../../shared/components/base/ScrollableList";
import BookCard from "../../../shared/components/BookCard";
import { useLastVisitedBooks } from "../hooks/api/useLastVisitedBooks";

const LastVisitedBooksList = (): ReactElement => {
  const { data, isLoading, isError, refetch } = useLastVisitedBooks();

  if (isLoading) {
    return <LoadingCard delayed itemName="Last Visited Books" small />;
  }

  if (isError) {
    return <RequestErrorCard itemName="Last Visited Books" onRetry={refetch} />;
  }

  if (data === undefined || data === null || data.totalCount === 0) {
    return <></>;
  }

  return (
    <ScrollableList
      title="Last Visited Books"
      itemWidth={data?.totalCount == 0 ? 450 : 150}
      itemGap={16}
    >
      {data?.items?.map((x) => (
        <BookCard key={x.id} book={x} small showRating={false} />
      ))}
    </ScrollableList>
  );
};

export default LastVisitedBooksList;
