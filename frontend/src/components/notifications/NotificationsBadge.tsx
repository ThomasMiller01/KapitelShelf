import type { BadgeProps } from "@mui/material";
import { Badge, styled } from "@mui/material";
import type { ReactNode } from "react";

import {
  type NotificationDto,
  NotificationSeverityDto,
} from "../../lib/api/KapitelShelf.Api";

const StyledBadge = styled(Badge)(() => ({
  "& .MuiBadge-badge": {
    border: `2px solid #323232`,
    padding: "0 4px",
  },
}));

interface NotificationsBadgeProps extends BadgeProps {
  notifications: NotificationDto[];
  children: ReactNode;
}

export const NotificationsBadge: React.FC<NotificationsBadgeProps> = ({
  notifications,
  children,
  ...props
}) => {
  const hasCritical =
    notifications.filter((x) => x.severity === NotificationSeverityDto.NUMBER_3)
      .length !== 0;

  return (
    <StyledBadge
      badgeContent={notifications?.length}
      color={hasCritical ? "error" : "primary"}
      max={9}
      {...props}
    >
      {children}
    </StyledBadge>
  );
};
