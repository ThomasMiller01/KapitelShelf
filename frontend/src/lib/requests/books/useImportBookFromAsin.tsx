import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useImportBookFromAsin = () => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (asin: string) => {
      const { data } = await clients.books.booksImportAsinPost(asin);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Importing book",
      },
    },
  });
};
