import { Box } from "@mui/material";
import { type ReactElement, useEffect, useRef } from "react";
import { useParams } from "react-router-dom";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import ItemAppBar from "../../components/base/ItemAppBar";
import NotificationDetails from "../../features/notifications/NotificationDetails";
import { useMarkNotificationAsRead } from "../../lib/requests/notifications/useMarkNotificationAsRead";
import { useNotificationById } from "../../lib/requests/notifications/useNotificationById";

const NotificationDetailPage = (): ReactElement => {
  const { notificationId } = useParams<{
    notificationId: string;
  }>();
  const {
    data: notification,
    isLoading,
    isError,
    refetch,
  } = useNotificationById(notificationId);

  const { mutateAsync: markNotificationAsRead } = useMarkNotificationAsRead();

  const previousChildrenCountRef = useRef<unknown | null>(null);
  useEffect(() => {
    if (!notification) {
      return;
    }

    // only mark as read when there are new children notifications
    const newChildren =
      JSON.stringify(previousChildrenCountRef.current) !==
      JSON.stringify(notification.children?.length);
    if (!newChildren) {
      return;
    }

    previousChildrenCountRef.current = notification.children?.length;
    markNotificationAsRead(notificationId);
  }, [notification]);

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
