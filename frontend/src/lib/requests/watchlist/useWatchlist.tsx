import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { useUserProfile } from "../../../hooks/useUserProfile";

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
