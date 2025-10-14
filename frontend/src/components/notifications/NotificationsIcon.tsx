import NotificationImportantIcon from "@mui/icons-material/NotificationImportant";
import NotificationsIconMUI from "@mui/icons-material/Notifications";
import NotificationsActiveIcon from "@mui/icons-material/NotificationsActive";
import type { SvgIconProps } from "@mui/material";

import {
  type NotificationDto,
  NotificationSeverityDto,
} from "../../lib/api/KapitelShelf.Api";

interface NotificationsIconProps extends SvgIconProps {
  notifications: NotificationDto[];
}

export const NotificationsIcon: React.FC<NotificationsIconProps> = ({
  notifications,
  ...props
}) => {
  if (notifications.length === 0) {
    return <NotificationsIconMUI {...props} />;
  }

  const hasCritical =
    notifications.filter((x) => x.severity === NotificationSeverityDto.NUMBER_3)
      .length !== 0;
  if (hasCritical) {
    return <NotificationImportantIcon color="error" {...props} />;
  }

  return <NotificationsActiveIcon color="primary" {...props} />;
};
