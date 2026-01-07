import NotificationImportantIcon from "@mui/icons-material/NotificationImportant";
import NotificationsIconMUI from "@mui/icons-material/Notifications";
import NotificationsActiveIcon from "@mui/icons-material/NotificationsActive";
import type { SvgIconProps } from "@mui/material";

import { useNotificationStats } from "../../hooks/useNotificationStats";

export const NotificationsIcon: React.FC<SvgIconProps> = ({ ...props }) => {
  const notificationStats = useNotificationStats();

  if (notificationStats?.unreadCount === 0) {
    return <NotificationsIconMUI {...props} />;
  }

  if (notificationStats?.unreadHasCritical) {
    return <NotificationImportantIcon color="error" {...props} />;
  }

  return <NotificationsActiveIcon color="primary" {...props} />;
};
