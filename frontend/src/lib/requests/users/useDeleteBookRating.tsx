import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

interface useDeleteBookRatingProps {
  bookId: string | undefined;
  userId: string | undefined;
}

export const useDeleteBookRating = ({
  bookId,
  userId,
}: useDeleteBookRatingProps) => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      if (bookId === undefined || userId === undefined) {
        return;
      }

      await clients.books.booksBookIdRatingUserIdDelete(bookId, userId);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["book-by-id", bookId] });
    },
  });
};
