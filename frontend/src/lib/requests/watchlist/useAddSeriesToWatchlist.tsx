import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { useUserProfile } from "../../../hooks/useUserProfile";

export const useAddSeriesToWatchlist = (
  seriesId: string | undefined,
  onSuccess = () => {}
) => {
  const { clients } = useApi();
  const { profile } = useUserProfile();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      if (seriesId === undefined || profile?.id === undefined) {
        return null;
      }

      await clients.watchlist.watchlistSeriesSeriesIdWatchPut(
        seriesId,
        profile.id
      );
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Adding series to watchlist",
        showLoading: false,
        showSuccess: false,
      },
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["series-is-on-watchlist", seriesId],
      });
      onSuccess?.();
    },
  });
};
