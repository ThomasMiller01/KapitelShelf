import type { ReactElement } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { ScrollableList } from "../../components/base/ScrollableList";
import BookCard from "../../components/BookCard";
import { useLastVisitedBooks } from "../../lib/requests/books/useLastVisitedBooks";

const LastVisitedBooksList = (): ReactElement => {
  const { data, isLoading, isError, refetch } = useLastVisitedBooks();

  if (isLoading) {
    return <LoadingCard delayed itemName="Last Visited Books" small />;
  }

  if (isError) {
    return <RequestErrorCard itemName="Last Visited Books" onRetry={refetch} />;
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
      {data?.totalCount === 0 && (
        <NoItemsFoundCard itemName="Last Visited Books" extraSmall />
      )}
    </ScrollableList>
  );
};

export default LastVisitedBooksList;
