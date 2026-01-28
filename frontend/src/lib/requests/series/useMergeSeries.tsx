import { useMutation } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

export const useMergeSeries = (seriesId: string | undefined) => {
  const { clients } = useApi();

  return useMutation({
    mutationFn: async (targetSeriesId: string | undefined) => {
      if (seriesId === undefined || targetSeriesId === undefined) {
        return null;
      }

      await clients.series.seriesSeriesIdMergeTargetSeriesIdPut(
        seriesId,
        targetSeriesId
      );
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Merging series",
        showLoading: true,
        showSuccess: true,
      },
    },
  });
};
