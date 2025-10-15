import DesktopWindowsRoundedIcon from "@mui/icons-material/DesktopWindowsRounded";
import InfoOutlineIcon from "@mui/icons-material/InfoOutline";
import ReportGmailerrorredIcon from "@mui/icons-material/ReportGmailerrorred";
import TaskAltIcon from "@mui/icons-material/TaskAlt";
import WarningRoundedIcon from "@mui/icons-material/WarningRounded";
import type { ReactNode } from "react";

import type { NotificationDto } from "../lib/api/KapitelShelf.Api";
import {
  NotificationSeverityDto,
  NotificationTypeDto,
} from "../lib/api/KapitelShelf.Api";

export const NotificationSeverityColor = (
  notification: NotificationDto
): string => {
  switch (notification.severity) {
    case NotificationSeverityDto.NUMBER_0:
    default:
      return "primary.dark";

    case NotificationSeverityDto.NUMBER_1:
      return "#A67C52";

    case NotificationSeverityDto.NUMBER_2:
      return "#D27A1A";

    case NotificationSeverityDto.NUMBER_3:
      return "error.main";
  }
};

export const NotificationTypeIcon = (
  notification: NotificationDto
): ReactNode => {
  switch (notification.type) {
    case NotificationTypeDto.NUMBER_0:
    default:
      return (
        <InfoOutlineIcon
          sx={{ color: NotificationSeverityColor(notification) }}
        />
      );

    case NotificationTypeDto.NUMBER_1:
      return (
        <TaskAltIcon sx={{ color: NotificationSeverityColor(notification) }} />
      );

    case NotificationTypeDto.NUMBER_2:
      return (
        <WarningRoundedIcon
          sx={{ color: NotificationSeverityColor(notification) }}
        />
      );

    case NotificationTypeDto.NUMBER_3:
      return (
        <ReportGmailerrorredIcon
          sx={{ color: NotificationSeverityColor(notification) }}
        />
      );

    case NotificationTypeDto.NUMBER_4:
      return (
        <DesktopWindowsRoundedIcon
          sx={{ color: NotificationSeverityColor(notification) }}
        />
      );
  }
};

export const NotificationTypeToString = (
  type: NotificationTypeDto | undefined
): string => {
  switch (type) {
    case NotificationTypeDto.NUMBER_0:
      return "Info";

    case NotificationTypeDto.NUMBER_1:
      return "Success";

    case NotificationTypeDto.NUMBER_2:
      return "Warning";

    case NotificationTypeDto.NUMBER_3:
      return "Error";

    case NotificationTypeDto.NUMBER_4:
      return "System";

    default:
      return "Unknown";
  }
};

export const NotificationSeverityToString = (
  type: NotificationSeverityDto | undefined
): string => {
  switch (type) {
    case NotificationSeverityDto.NUMBER_0:
      return "Low";

    case NotificationSeverityDto.NUMBER_1:
      return "Medium";

    case NotificationSeverityDto.NUMBER_2:
      return "High";

    case NotificationSeverityDto.NUMBER_3:
      return "Critical";

    default:
      return "Unknown";
  }
};
