import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { useUserProfile } from "../../../../shared/hooks/useUserProfile";
import { SECOND_MS } from "../../../../shared/utils/TimeUtils";

export const useNotificationStatsPoller = () => {
  const { profile } = useUserProfile();
  const { clients } = useApi();

  useQuery({
    queryKey: ["notifications-stats", profile?.id],
    enabled: !!profile?.id,
    queryFn: async () => {
      const { data } = await clients.notifications.notificationsStatsGet(
        profile?.id
      );
      return data;
    },
    refetchInterval: 30 * SECOND_MS,
    staleTime: Infinity,
  });
};
