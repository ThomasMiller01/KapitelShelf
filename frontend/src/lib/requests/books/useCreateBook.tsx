import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { BookDTO } from "../../api/KapitelShelf.Api";

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
