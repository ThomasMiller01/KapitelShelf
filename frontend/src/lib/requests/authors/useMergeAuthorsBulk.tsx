import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

interface mutationFnProps {
  targetAuthorId: string;
  sourceAuthorsIds: string[];
}

export const useMergeAuthorsBulk = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      targetAuthorId,
      sourceAuthorsIds,
    }: mutationFnProps) => {
      await clients.authors.authorsAuthorIdMergePut(
        targetAuthorId,
        sourceAuthorsIds,
      );
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Merging authors",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      await queryClient.invalidateQueries({ queryKey: ["authors"] }),
  });
};
