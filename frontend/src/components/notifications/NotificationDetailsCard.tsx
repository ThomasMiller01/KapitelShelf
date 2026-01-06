import {
  Badge,
  BadgeProps,
  Box,
  Card,
  CardContent,
  Stack,
  styled,
  Tooltip,
  Typography,
} from "@mui/material";

import { useMobile } from "../../hooks/useMobile";
import { type NotificationDto } from "../../lib/api/KapitelShelf.Api";
import { GetColor } from "../../utils/ColorUtils";
import {
  NotificationSeverityColor,
  NotificationSeverityToString,
  NotificationTypeIcon,
  NotificationTypeToString,
} from "../../utils/NotificationUtils";
import { FormatTimeUntil } from "../../utils/TimeUtils";
import { Property } from "../base/Property";

export interface TitleBadgeProps extends BadgeProps {
  badgeColor?: string;
}

const TitleBadge = styled(Badge, {
  shouldForwardProp: (prop) => prop !== "badgeColor",
})<TitleBadgeProps>(({ theme, badgeColor }) => ({
  "& .MuiBadge-badge": {
    right: -20,
    top: 10,
    padding: "0 4px",
    backgroundColor: GetColor(badgeColor, theme),
  },
}));

interface NotificationDetailsCardProps {
  notification: NotificationDto;
}

export const NotificationDetailsCard: React.FC<
  NotificationDetailsCardProps
> = ({ notification }) => {
  // TODO: implement proper details card
  const { isMobile } = useMobile();
  return (
    <Card
      variant="outlined"
      sx={{
        display: "flex",
        borderLeft: `5px solid`,
        borderLeftColor: NotificationSeverityColor(notification),
      }}
    >
      <Box
        sx={{
          width: "100%",
          display: "flex",
          justifyContent: "space-between",
          paddingRight: isMobile ? "0" : "10px",
        }}
      >
        <CardContent sx={{ padding: "6px 10px !important" }}>
          <TitleBadge
            badgeContent={notification.children?.length}
            max={9}
            color="primary"
            badgeColor={NotificationSeverityColor(notification)}
          >
            <Stack direction="row" spacing={1.1} alignItems="start">
              <Box pt="2px">
                <Tooltip
                  arrow
                  disableInteractive
                  placement="top"
                  title={
                    <Stack direction="row" alignItems="baseline" spacing={1}>
                      <Typography variant="subtitle1">
                        {NotificationTypeToString(notification.type)}
                      </Typography>
                      <Typography
                        variant="overline"
                        sx={{
                          lineHeight: 1.5,
                          fontWeight: "bold",
                          color: NotificationSeverityColor(notification),
                        }}
                      >
                        [{NotificationSeverityToString(notification.severity)}]
                      </Typography>
                    </Stack>
                  }
                >
                  <Box>{NotificationTypeIcon(notification)}</Box>
                </Tooltip>
              </Box>
              <Stack>
                <Typography variant="subtitle1" component="div">
                  {notification.title}
                </Typography>
                <Typography variant="body2" sx={{ color: "text.secondary" }}>
                  {notification.message}
                </Typography>
              </Stack>
            </Stack>
          </TitleBadge>
        </CardContent>

        <Property label="Expires In" tooltip={notification.expires}>
          <Typography>
            {FormatTimeUntil(notification.expires, false)}
          </Typography>
        </Property>

        <Property label="Source">
          <Typography variant="overline">{notification.source}</Typography>
        </Property>
      </Box>
    </Card>
  );
};
