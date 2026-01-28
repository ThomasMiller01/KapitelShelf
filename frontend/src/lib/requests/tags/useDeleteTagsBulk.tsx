import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useDeleteTagsBulk = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (tagIdsToDelete: string[]) => {
      await clients.tags.tagsDelete(tagIdsToDelete);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Deleting tags",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      queryClient.invalidateQueries({ queryKey: ["tags"] }),
  });
};
