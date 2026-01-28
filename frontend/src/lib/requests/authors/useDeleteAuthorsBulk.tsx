import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useDeleteAuthorsBulk = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (authorsIdsToDelete: string[]) => {
      await clients.authors.authorsDelete(authorsIdsToDelete);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Deleting authors",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      queryClient.invalidateQueries({ queryKey: ["authors"] }),
  });
};
