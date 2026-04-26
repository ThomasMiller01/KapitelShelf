import { useQuery, type UseQueryResult } from "@tanstack/react-query";

import type {
  ReadingBookDTO,
  ReadingLocationDTO,
} from "../../../../lib/api/KapitelShelf.Api";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { useUserProfile } from "../../../../shared/hooks/useUserProfile";

const NO_READING_LOCATION = null as unknown as ReadingLocationDTO | undefined;

export const useMarkReadingBook = (
  bookId: string | undefined,
): UseQueryResult<ReadingBookDTO | undefined> => {
  const { clients } = useApi();
  const { profile } = useUserProfile();

  return useQuery({
    queryKey: ["mark-reading-book", bookId, profile?.id],
    queryFn: async () => {
      if (bookId === undefined || profile?.id === undefined) {
        return;
      }

      const { data } = await clients.books.booksBookIdReadingPut(
        bookId,
        profile.id,
        NO_READING_LOCATION,
      );

      return data;
    },
    enabled: Boolean(bookId) && Boolean(profile?.id),
    refetchInterval: false,
  });
};
