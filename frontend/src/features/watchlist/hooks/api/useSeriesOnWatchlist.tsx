import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { useUserProfile } from "../../../../shared/hooks/useUserProfile";
import { SeriesSupportsWatchlist } from "../../utils/WatchlistUtils";
import { SeriesDTO } from "../../../../lib/api/KapitelShelf.Api/index.ts";

export const useSeriesOnWatchlist = (series: SeriesDTO | undefined | null) => {
  const { clients } = useApi();
  const { profile } = useUserProfile();

  return useQuery({
    queryKey: ["series-is-on-watchlist", series?.id],
    queryFn: async () => {
      if (series?.id === undefined || profile?.id === undefined) {
        return null;
      }

      const { data } =
        await clients.watchlist.watchlistSeriesSeriesIdIswatchedGet(
          series.id,
          profile.id
        );
      return data;
    },
    enabled: SeriesSupportsWatchlist(series),
  });
};
