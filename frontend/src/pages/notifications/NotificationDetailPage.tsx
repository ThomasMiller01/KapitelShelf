import { Box } from "@mui/material";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { type ReactElement, useEffect } from "react";
import { useParams } from "react-router-dom";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import ItemAppBar from "../../components/base/ItemAppBar";
import { useApi } from "../../contexts/ApiProvider";
import NotificationDetails from "../../features/notifications/NotificationDetails";
import { useUserProfile } from "../../hooks/useUserProfile";

const NotificationDetailPage = (): ReactElement => {
  const { notificationId } = useParams<{
    notificationId: string;
  }>();
  const { clients } = useApi();
  const queryClient = useQueryClient();
  const { profile } = useUserProfile();

  const {
    data: notification,
    isLoading,
    isError,
    refetch,
  } = useQuery({
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
  });

  const { mutateAsync: markNotificationAsRead } = useMutation({
    mutationKey: ["notification-id-mark-as-read", notificationId],
    mutationFn: async () => {
      if (notificationId === undefined) {
        return null;
      }

      await clients.notifications.notificationsIdReadPost(
        notificationId,
        profile?.id
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["notifications-stats", profile?.id],
      });
    },
  });

  useEffect(() => {
    markNotificationAsRead();
  }, []);

  if (isLoading) {
    return (
      <LoadingCard useLogo delayed itemName="Notification" showRandomFacts />
    );
  }

  if (isError || notification === undefined || notification === null) {
    return <RequestErrorCard itemName="notification" onRetry={refetch} />;
  }

  return (
    <Box>
      <ItemAppBar backTooltip="Go to notifications" backUrl="/notifications" />
      <NotificationDetails notification={notification} />
    </Box>
  );
};

export default NotificationDetailPage;
