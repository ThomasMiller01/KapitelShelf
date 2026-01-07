import DoneAllIcon from "@mui/icons-material/DoneAll";
import { Box, Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import { type ReactElement } from "react";

import { ButtonWithTooltip } from "../../components/base/ButtonWithTooltip";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { NotificationsBadge } from "../../components/notifications/NotificationsBadge";
import { useApi } from "../../contexts/ApiProvider";
import { NotificationsList } from "../../features/notifications/NotificationsList";
import { useMobile } from "../../hooks/useMobile";
import { useUserProfile } from "../../hooks/useUserProfile";

export const NotificationsPage = (): ReactElement => {
  const { isMobile } = useMobile();

  const { clients } = useApi();
  const { profile } = useUserProfile();

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ["notifications-list", profile?.id],
    queryFn: async () => {
      if (profile?.id === undefined) {
        return null;
      }

      const { data } = await clients.notifications.notificationsGet(
        profile?.id
      );
      return data;
    },
  });

  if (isLoading) {
    return <LoadingCard delayed itemName="Notifications" small />;
  }

  if (isError) {
    return <RequestErrorCard itemName="Notifications" onRetry={refetch} />;
  }

  if (data?.length === 0) {
    return <NoItemsFoundCard itemName="Notifications" small />;
  }

  return (
    <Box padding={isMobile ? "10px" : "20px"} paddingTop="20px">
      <NotificationsBadge
        sx={{
          "& .MuiBadge-badge": {
            border: `2px solid #121212`,
            right: -10,
          },
        }}
      >
        <Typography variant="h5">Notifications</Typography>
      </NotificationsBadge>
      <Box display="flex" justifyContent="end">
        <ButtonWithTooltip
          tooltip="Mark all as Read"
          startIcon={<DoneAllIcon />}
        >
          Read All
        </ButtonWithTooltip>
      </Box>
      <NotificationsList notifications={data ?? []} />
    </Box>
  );
};
