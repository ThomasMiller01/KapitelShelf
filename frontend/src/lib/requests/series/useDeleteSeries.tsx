import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useDeleteSeries = (seriesId: string | undefined) => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async () => {
      if (seriesId === undefined) {
        return null;
      }

      await clients.series.seriesSeriesIdDelete(seriesId);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Deleting series",
        showLoading: true,
        showSuccess: true,
      },
    },
  });
};
