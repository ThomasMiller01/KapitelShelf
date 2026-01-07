import { useQuery } from "@tanstack/react-query";
import { useApi } from "../contexts/ApiProvider";
import { NotificationStatsDto } from "../lib/api/KapitelShelf.Api";
import { useUserProfile } from "./useUserProfile";

export const useNotificationStats = ():
  | NotificationStatsDto
  | null
  | undefined => {
  const { profile } = useUserProfile();
  const { clients } = useApi();

  const { data } = useQuery({
    queryKey: ["notifications-stats", profile?.id],
    enabled: !!profile?.id,
    queryFn: async () => {
      if (!profile?.id) {
        return null;
      }

      const { data } = await clients.notifications.notificationsStatsGet(
        profile.id
      );
      return data;
    },

    // get data from cache, NotificationStatsPoller is handling the re-fetching
    staleTime: Infinity,
    refetchInterval: false,
  });

  return data;
};
