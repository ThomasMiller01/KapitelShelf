import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { useUserProfile } from "../../../hooks/useUserProfile";
import { SeriesSupportsWatchlist } from "../../../utils/WatchlistUtils";
import { SeriesDTO } from "../../api/KapitelShelf.Api";

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
