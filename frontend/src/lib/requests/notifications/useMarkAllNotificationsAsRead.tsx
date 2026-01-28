import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../../contexts/ApiProvider";
import { useUserProfile } from "../../../hooks/useUserProfile";

export const useMarkAllNotifictaionsAsRead = () => {
  const { clients } = useApi();
  const { profile } = useUserProfile();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async () => {
      if (profile?.id === undefined) {
        return;
      }

      await clients.notifications.notificationsReadallPost(profile.id);
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
        operation: `Marking all notification as read`,
        showLoading: true,
        showSuccess: false,
        showError: true,
      },
    },
  });
};
