import CategoryIcon from "@mui/icons-material/Category";
import DoneAllIcon from "@mui/icons-material/DoneAll";
import ReportProblemIcon from "@mui/icons-material/ReportProblem";
import { Box, Stack, Typography } from "@mui/material";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState, type ReactElement } from "react";

import { ButtonWithTooltip } from "../../components/base/ButtonWithTooltip";
import LoadingCard from "../../components/base/feedback/LoadingCard";
import { NoItemsFoundCard } from "../../components/base/feedback/NoItemsFoundCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import { HiddenFilter } from "../../components/base/HiddenFilter";
import { NotificationsBadge } from "../../components/notifications/NotificationsBadge";
import { useApi } from "../../contexts/ApiProvider";
import { NotificationsList } from "../../features/notifications/NotificationsList";
import { useMobile } from "../../hooks/useMobile";
import { useNotificationStats } from "../../hooks/useNotificationStats";
import { useUserProfile } from "../../hooks/useUserProfile";
import { NotificationDto } from "../../lib/api/KapitelShelf.Api";
import {
  NotificationSeverityStringToDto,
  NotificationSeverityToColor,
  NotificationSeverityToString,
  NotificationTypeToString,
} from "../../utils/NotificationUtils";

export const NotificationsPage = (): ReactElement => {
  const { isMobile } = useMobile();

  const { clients } = useApi();
  const { profile } = useUserProfile();
  const queryClient = useQueryClient();

  const notificationStats = useNotificationStats();

  const {
    data: notifications,
    isLoading,
    isError,
    refetch,
  } = useQuery({
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

  const [hiddenTypes, setHiddenTypes] = useState<string[]>([]);
  const [hiddenSeverities, setHiddenSeverities] = useState<string[]>([]);

  const filteredNotifications = useMemo<NotificationDto[]>(() => {
    const list = notifications ?? [];

    return list.filter((x) => {
      const severity = NotificationSeverityToString(x.severity);
      const type = NotificationTypeToString(x.type);

      return (
        !hiddenSeverities.includes(severity) && !hiddenTypes.includes(type)
      );
    });
  }, [notifications, hiddenSeverities, hiddenTypes]);

  const { mutate: markAllAsRead } = useMutation({
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

  if (isLoading) {
    return <LoadingCard delayed itemName="Notifications" small />;
  }

  if (isError) {
    return <RequestErrorCard itemName="Notifications" onRetry={refetch} />;
  }

  if (notifications?.length === 0) {
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
      <Stack direction="row" spacing={2} justifyContent="end">
        {/* Severity Filter */}
        <HiddenFilter
          items={notifications ?? []}
          extractValue={(x) => NotificationSeverityToString(x.severity)}
          onHiddenOptionsChange={(next) => setHiddenSeverities(next)}
          settingsKey="notifications.hide.severity"
          subIcon={<ReportProblemIcon fontSize="small" />}
          tooltip="Hide notifications by severity"
          textColor={(x) =>
            NotificationSeverityToColor(NotificationSeverityStringToDto(x))
          }
        />

        {/* Type Filter */}
        <HiddenFilter
          items={notifications ?? []}
          extractValue={(x) => NotificationTypeToString(x.type)}
          onHiddenOptionsChange={(next) => setHiddenTypes(next)}
          defaultHiddenOptions={["System"]}
          settingsKey="notifications.hide.type"
          subIcon={<CategoryIcon fontSize="small" />}
          tooltip="Hide notifications by type"
        />

        {/* Read All */}
        <ButtonWithTooltip
          tooltip="Mark all as Read"
          startIcon={<DoneAllIcon />}
          onClick={() => markAllAsRead()}
          disabled={notificationStats?.unreadCount === 0}
          disabledTooltip="All notifications are already read"
        >
          Read All
        </ButtonWithTooltip>
      </Stack>
      <NotificationsList notifications={filteredNotifications ?? []} />
    </Box>
  );
};
