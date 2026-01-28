import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

interface mutationFnProps {
  targetTagId: string;
  sourceTagsIds: string[];
}

export const useMergeTagsBulk = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ targetTagId, sourceTagsIds }: mutationFnProps) => {
      await clients.tags.tagsTagIdMergePut(targetTagId, sourceTagsIds);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Merging tags",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      await queryClient.invalidateQueries({ queryKey: ["tags"] }),
  });
};
