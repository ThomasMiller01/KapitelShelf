import { useQuery } from "@tanstack/react-query";
import { useApi } from "../../../../shared/contexts/ApiProvider";
import { useUserProfile } from "../../../../shared/hooks/useUserProfile";
import { SECOND_MS } from "../../../../shared/utils/TimeUtils";

export const useNotificationById = (notificationId: string | undefined) => {
  const { clients } = useApi();
  const { profile } = useUserProfile();

  return useQuery({
    queryKey: ["notification-by-id", notificationId],
    queryFn: async () => {
      if (notificationId === undefined) {
        return null;
      }

      const { data } = await clients.notifications.notificationsIdGet(
        notificationId,
        profile?.id
      );
      return data;
    },
    refetchInterval: 10 * SECOND_MS,
  });
};
