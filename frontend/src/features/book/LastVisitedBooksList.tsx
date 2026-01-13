import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { ScrollableList } from "../../components/base/ScrollableList";
import BookCard from "../../components/BookCard";
import { useApi } from "../../contexts/ApiProvider";
import { useUserProfile } from "../../hooks/useUserProfile";

const PAGE_SIZE = 24;

const LastVisitedBooksList = (): ReactElement => {
  const { clients } = useApi();
  const { profile } = useUserProfile();

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ["last-visited-books", profile?.id],
    queryFn: async () => {
      if (profile?.id === undefined) {
        return null;
      }

      const { data } = await clients.users.usersUserIdLastvisitedbooksGet(
        profile?.id,
        1,
        PAGE_SIZE
      );
      return data;
    },
  });

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
        <BookCard key={x.id} book={x} small />
      ))}
      {data?.totalCount === 0 && (
        <NoItemsFoundCard itemName="Last Visited Books" extraSmall />
      )}
    </ScrollableList>
  );
};

export default LastVisitedBooksList;
