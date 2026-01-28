import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useDeleteBooks = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (bookIdsToDelete: string[]) => {
      await clients.books.booksDelete(bookIdsToDelete);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Deleting  books",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      await queryClient.invalidateQueries({ queryKey: ["books"] }),
  });
};
