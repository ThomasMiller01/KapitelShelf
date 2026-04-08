import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { useUserProfile } from "../../../../shared/hooks/useUserProfile";

export const useWatchlist = () => {
  const { profile } = useUserProfile();
  const { clients } = useApi();

  return useQuery({
    queryKey: ["watchlist", profile?.id],
    queryFn: async () => {
      if (profile?.id === undefined) {
        return null;
      }

      const { data } = await clients.watchlist.watchlistGet(profile?.id);
      return data;
    },
  });
};
