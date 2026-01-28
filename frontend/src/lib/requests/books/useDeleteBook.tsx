import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useDeleteBook = (bookId: string | undefined) => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async () => {
      if (bookId === undefined) {
        return null;
      }

      await clients.books.booksBookIdDelete(bookId);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Deleting book",
        showLoading: true,
        showSuccess: true,
      },
    },
  });
};
