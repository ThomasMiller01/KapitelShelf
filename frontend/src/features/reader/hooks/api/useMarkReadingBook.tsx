import { useMutation } from "@tanstack/react-query";
import { useEffect } from "react";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { useUserProfile } from "../../../../shared/hooks/useUserProfile";

export const useMarkReadingBook = (bookId: string | undefined) => {
  const { clients } = useApi();
  const { profile } = useUserProfile();

  const { mutate } = useMutation({
    mutationFn: async () => {
      if (bookId === undefined || profile?.id === undefined) {
        return;
      }

      await clients.books.booksBookIdReadingPut(bookId, profile?.id);
    },
  });

  // mark book as reading on loading the reader with this book
  useEffect(() => mutate(), [bookId, profile?.id]);
};
