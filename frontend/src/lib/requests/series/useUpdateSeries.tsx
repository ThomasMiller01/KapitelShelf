import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { SeriesDTO } from "../../api/KapitelShelf.Api";

export const useUpdateSeries = (seriesId: string | undefined) => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (series: SeriesDTO) => {
      if (seriesId === undefined) {
        return null;
      }

      await clients.series.seriesSeriesIdPut(seriesId, series);
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Updating series",
        showLoading: true,
        showSuccess: true,
      },
    },
  });
};
