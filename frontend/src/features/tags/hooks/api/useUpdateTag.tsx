import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { TagDTO } from "../../../../lib/api/KapitelShelf.Api/index.ts";

export const useUpdateTag = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (tag: TagDTO) => {
      if (tag.id === undefined) {
        return null;
      }

      await clients.tags.tagsTagIdPut(tag.id, tag);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Updating tag",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      queryClient.invalidateQueries({ queryKey: ["tags"] }),
  });
};
