import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useAiGenerateCategoriesTags = () => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (bookId: string | undefined) => {
      if (bookId === undefined) {
        return;
      }

      const { data } =
        await clients.books.booksBookIdAiGenerateCategoriesTagsGet(bookId);
      return data;
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Generating Tags & Categories with AI",
        showLoading: true,
        showSuccess: true,
      },
    },
  });
};
