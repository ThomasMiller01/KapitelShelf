import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { CreateOrUpdateUserBookMetadataDTO } from "../../../../lib/api/KapitelShelf.Api/index.ts";

interface useAddOrUpdateBookRatingProps {
  bookId: string | undefined;
  userId: string | undefined;
}

export const useAddOrUpdateBookRating = ({
  bookId,
  userId,
}: useAddOrUpdateBookRatingProps) => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (bookRating: CreateOrUpdateUserBookMetadataDTO) => {
      if (bookId === undefined || userId === undefined) {
        return;
      }

      await clients.books.booksBookIdRatingUserIdPut(
        bookId,
        userId,
        bookRating,
      );
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["book-by-id", bookId] });
    },
  });
};
