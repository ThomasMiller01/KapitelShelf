import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { BookDTO } from "../../api/KapitelShelf.Api";

export const useUpdateBook = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (book: BookDTO) => {
      if (book.id === undefined) {
        return null;
      }

      await clients.books.booksBookIdPut(book.id, book);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Updating book",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      queryClient.invalidateQueries({ queryKey: ["books"] }),
  });
};
