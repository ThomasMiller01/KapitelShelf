import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { useUserProfile } from "../../../hooks/useUserProfile";

export const useRemoveSeriesFromWatchlist = (seriesId: string | undefined) => {
  const { clients } = useApi();
  const { profile } = useUserProfile();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      if (seriesId === undefined || profile?.id === undefined) {
        return null;
      }

      await clients.watchlist.watchlistSeriesSeriesIdWatchDelete(
        seriesId,
        profile.id
      );
    },
    meta: {
      notify: {
        enabled: true,
        operation: "Removing series from watchlist",
        showLoading: false,
        showSuccess: true,
      },
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["series-is-on-watchlist", seriesId],
      });
    },
  });
};
