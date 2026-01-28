import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

interface mutationFnProps {
  targetCategoryId: string;
  sourceCategoryIds: string[];
}

export const useMergeCategoriesBulk = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      targetCategoryId,
      sourceCategoryIds,
    }: mutationFnProps) => {
      await clients.categories.categoriesCategoryIdMergePut(
        targetCategoryId,
        sourceCategoryIds,
      );
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Merging categories",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      await queryClient.invalidateQueries({ queryKey: ["categories"] }),
  });
};
