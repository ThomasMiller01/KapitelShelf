import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { CategoryDTO } from "../../api/KapitelShelf.Api";

export const useUpdateCategory = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (category: CategoryDTO) => {
      if (category.id === undefined) {
        return null;
      }

      await clients.categories.categoriesCategoryIdPut(category.id, category);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Updating category",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      queryClient.invalidateQueries({ queryKey: ["categories"] }),
  });
};
