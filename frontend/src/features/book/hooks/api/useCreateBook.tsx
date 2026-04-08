import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { BookDTO } from "../../../../lib/api/KapitelShelf.Api/index.ts";

export const useCreateBook = () => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (book: BookDTO) => {
      const { data } = await clients.books.booksPost(book);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Creating Book",
      },
    },
  });
};
