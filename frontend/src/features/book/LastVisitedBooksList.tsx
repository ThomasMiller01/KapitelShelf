import { useQuery } from "@tanstack/react-query";
import type { ReactElement } from "react";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import BookCard from "../../components/BookCard";
import { ScrollableList } from "../../components/ScrollableList";
import { useUserProfile } from "../../hooks/useUserProfile";
import { usersApi } from "../../lib/api/KapitelShelf.Api";

const PAGE_SIZE = 24;

const LastVisitedBooksList = (): ReactElement => {
  const { profile } = useUserProfile();

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ["last-visited-books", profile?.id],
    queryFn: async () => {
      if (profile?.id === undefined) {
        return null;
      }

      const { data } = await usersApi.usersUserIdLastvisitedbooksGet(
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

  if (data?.totalCount === 0) {
    return <NoItemsFoundCard itemName="Last Visited Books" small />;
  }

  return (
    <ScrollableList title="Last Visited Books" itemWidth={150} itemGap={16}>
      {data?.items?.map((x) => (
        <BookCard key={x.id} book={x} small />
      ))}
    </ScrollableList>
  );
};

export default LastVisitedBooksList;
