import { Stack } from "@mui/material";
import { NotificationDetailsCard } from "../../components/notifications/NotificationDetailsCard";
import type { NotificationDto } from "../../lib/api/KapitelShelf.Api/api";

interface NotificationDetailsProps {
  notification: NotificationDto;
}

const NotificationDetails: React.FC<NotificationDetailsProps> = ({
  notification,
}) => {
  return (
    <Stack spacing={1} p={3}>
      {notification.children?.map((child) => (
        <NotificationDetailsCard key={child.id} notification={child} />
      ))}
      <NotificationDetailsCard notification={notification} />
    </Stack>
  );
};

export default NotificationDetails;
