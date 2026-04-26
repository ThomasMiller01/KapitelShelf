import { useQuery } from "@tanstack/react-query";
import type { ReadingLocationDTO } from "../../../../lib/api/KapitelShelf.Api";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { useUserProfile } from "../../../../shared/hooks/useUserProfile";

export const useMarkReadingBook = (bookId: string | undefined) => {
  const { clients } = useApi();
  const { profile } = useUserProfile();

  return useQuery({
    queryKey: ["mark-reading-book", bookId, profile?.id],
    queryFn: async () => {
      if (bookId === undefined || profile?.id === undefined) {
        return;
      }

      const noReadingLocation =
        null as unknown as ReadingLocationDTO | undefined;

      await clients.books.booksBookIdReadingPut(
        bookId,
        profile?.id,
        noReadingLocation,
      );
    },
    enabled: !!profile?.id,
    refetchInterval: false,
  });
};
