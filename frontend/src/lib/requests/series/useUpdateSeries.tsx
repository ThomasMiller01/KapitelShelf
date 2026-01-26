import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { SeriesDTO } from "../../api/KapitelShelf.Api";

export const useUpdateSeries = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (series: SeriesDTO) => {
      if (series.id === undefined) {
        return null;
      }

      await clients.series.seriesSeriesIdPut(series.id, series);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Updating series",
        showLoading: true,
        showSuccess: true,
      },
    },
    onSuccess: async () =>
      queryClient.invalidateQueries({ queryKey: ["series"] }),
  });
};
