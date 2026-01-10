import { Stack } from "@mui/material";
import {
  NotificationCard,
  NotificationCardProps,
} from "../../components/notifications/NotificationCard";
import type { NotificationDto } from "../../lib/api/KapitelShelf.Api/api";

interface NotificationDetailsProps {
  notification: NotificationDto;
}

const NotificationDetails: React.FC<NotificationDetailsProps> = ({
  notification,
}) => {
  return (
    <Stack spacing={1} p={{ xs: 1, sm: 3 }}>
      {notification.children?.map((child) => (
        <DetailNotificationCard key={child.id} notification={child} />
      ))}
      <DetailNotificationCard notification={notification} />
    </Stack>
  );
};

const DetailNotificationCard: React.FC<NotificationCardProps> = (props) => {
  return (
    <NotificationCard
      {...props}
      hideReadStatus
      hideChildCount
      disableLink
      hideActions
      showDetails
    />
  );
};

export default NotificationDetails;
