import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useDeleteSeriesBulk = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (seriesIdToDelete: string[]) => {
      await clients.series.seriesDelete(seriesIdToDelete);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Deleting series",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      queryClient.invalidateQueries({ queryKey: ["series"] }),
  });
};
