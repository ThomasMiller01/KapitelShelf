import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { useUserProfile } from "../../../hooks/useUserProfile";

export const useBookById = (bookId: string | undefined) => {
  const { clients } = useApi();
  const { profile } = useUserProfile();

  return useQuery({
    queryKey: ["book-by-id", bookId],
    queryFn: async () => {
      if (bookId === undefined) {
        return null;
      }

      const { data } = await clients.books.booksBookIdGet(bookId, profile?.id);
      return data;
    },
  });
};
