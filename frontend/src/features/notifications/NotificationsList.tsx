import { Stack } from "@mui/material";

import { NotificationCard } from "../../components/notifications/NotificationCard";
import type { NotificationDto } from "../../lib/api/KapitelShelf.Api";

interface NotificationsListProps {
  notifications: NotificationDto[];
}

export const NotificationsList: React.FC<NotificationsListProps> = ({
  notifications,
}) => (
  <Stack spacing={1} sx={{ my: 2, mb: 3 }}>
    {notifications.map((x) => (
      <NotificationCard key={x.id} notification={x} />
    ))}
  </Stack>
);
