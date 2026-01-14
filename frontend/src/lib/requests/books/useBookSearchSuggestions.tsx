import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useBookSearchSuggestions = () => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (term: string) => {
      if (term === "") {
        return [];
      }

      const { data } = await clients.books.booksSearchSuggestionsGet(term);
      return data;
    },
  });
};
