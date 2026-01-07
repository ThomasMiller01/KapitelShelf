import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../contexts/ApiProvider";
import { useUserProfile } from "../../hooks/useUserProfile";
import { SECOND_MS } from "../../utils/TimeUtils";

export const NotificationsStatsPoller: React.FC = () => {
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

  return null;
};
