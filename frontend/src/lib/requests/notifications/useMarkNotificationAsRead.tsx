import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { useUserProfile } from "../../../hooks/useUserProfile";

export const useMarkNotificationAsRead = () => {
  const { clients } = useApi();
  const { profile } = useUserProfile();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (notificationId: string | undefined) => {
      if (profile?.id === undefined || notificationId === undefined) {
        return;
      }

      await clients.notifications.notificationsIdReadPost(
        notificationId,
        profile.id
      );
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: ["notifications-list", profile?.id],
        }),
        queryClient.invalidateQueries({
          queryKey: ["notifications-stats", profile?.id],
        }),
      ]);
    },
    meta: {
      notify: {
        enabled: true,
        operation: `Marking notification as read`,
        showLoading: true,
        showSuccess: false,
        showError: true,
      },
    },
  });
};
