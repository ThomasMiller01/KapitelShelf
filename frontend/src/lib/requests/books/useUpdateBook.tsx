import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { BookDTO } from "../../api/KapitelShelf.Api";

export const useUpdateBook = (bookId: string | undefined) => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (book: BookDTO) => {
      if (bookId === undefined) {
        return null;
      }

      await clients.books.booksBookIdPut(bookId, book);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Updating book",
        showLoading: true,
        showSuccess: true,
      },
    },
  });
};
