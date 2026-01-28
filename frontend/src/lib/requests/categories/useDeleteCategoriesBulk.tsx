import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useDeleteCategoriesBulk = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (categoryIdsToDelete: string[]) => {
      await clients.categories.categoriesDelete(categoryIdsToDelete);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Deleting categories",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      queryClient.invalidateQueries({ queryKey: ["categories"] }),
  });
};
