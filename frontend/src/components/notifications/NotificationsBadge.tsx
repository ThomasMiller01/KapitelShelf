import type { BadgeProps } from "@mui/material";
import { Badge, styled } from "@mui/material";
import type { ReactNode } from "react";

import { useNotificationStats } from "../../hooks/useNotificationStats";

const StyledBadge = styled(Badge)(() => ({
  "& .MuiBadge-badge": {
    border: `2px solid #323232`,
    padding: "0 4px",
  },
}));

interface NotificationsBadgeProps extends BadgeProps {
  children: ReactNode;
}

export const NotificationsBadge: React.FC<NotificationsBadgeProps> = ({
  children,
  ...props
}) => {
  const notificationStats = useNotificationStats();

  return (
    <StyledBadge
      badgeContent={notificationStats?.unreadCount}
      color={notificationStats?.unreadHasCritical ? "error" : "primary"}
      max={9}
      {...props}
    >
      {children}
    </StyledBadge>
  );
};
