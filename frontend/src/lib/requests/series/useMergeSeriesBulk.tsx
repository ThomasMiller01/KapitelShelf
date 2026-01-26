import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";

interface mutationFnProps {
  targetSeriesId: string;
  sourceSeriesIds: string[];
}

export const useMergeSeriesBulk = () => {
  const { clients } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      targetSeriesId,
      sourceSeriesIds,
    }: mutationFnProps) => {
      // if (seriesId === undefined || targetSeriesId === undefined) {
      //   return null;
      // }

      await clients.series.seriesSeriesIdMergeBulkPut(
        targetSeriesId,
        sourceSeriesIds,
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
    onSuccess: async () =>
      await queryClient.invalidateQueries({ queryKey: ["series"] }),
  });
};
