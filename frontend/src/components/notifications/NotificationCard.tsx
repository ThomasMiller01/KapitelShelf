import CheckIcon from "@mui/icons-material/Check";
import {
  Badge,
  BadgeProps,
  Box,
  Card,
  CardActionArea,
  CardActions,
  CardContent,
  Container,
  Stack,
  styled,
  Tooltip,
  Typography,
} from "@mui/material";

import { Link } from "react-router-dom";
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
import { ButtonWithTooltip } from "../base/ButtonWithTooltip";
import { Property } from "../base/Property";

export interface TitleBadgeProps extends BadgeProps {
  badgeColor?: string;
}

const TitleBadge = styled(Badge, {
  shouldForwardProp: (prop) => prop !== "badgeColor",
})<TitleBadgeProps>(({ theme, badgeColor, badgeContent }) => ({
  paddingRight: 20,
  "& .MuiBadge-badge": {
    right: 0,
    top: 0,
    transform: "translateX(8px)",
    display: badgeContent ? "inline-block" : "none", // required because of the transform
    borderRadius: 6,
    backgroundColor: GetColor(badgeColor, theme),
  },
}));

interface NotificationCardProps {
  notification: NotificationDto;
}

export const NotificationCard: React.FC<NotificationCardProps> = ({
  notification,
}) => {
  const { isMobile } = useMobile();
  return (
    <Card
      variant="outlined"
      sx={{
        display: "flex",
        borderLeft: `5px solid`,
        borderLeftColor: NotificationSeverityColor(notification),
        opacity: notification.isRead ? 0.6 : 1,
      }}
    >
      <CardActionArea
        component={Link}
        to={`/notifications/${notification.id}`}
        sx={{
          paddingRight: isMobile ? "0" : "10px",
        }}
      >
        <CardContent
          sx={{
            padding: "6px 10px !important",
            display: "flex",
            gap: "15px",
            justifyContent: "space-between",
          }}
        >
          <Container disableGutters maxWidth="sm" sx={{ m: 0 }}>
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
                          [{NotificationSeverityToString(notification.severity)}
                          ]
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
          </Container>

          {!isMobile && (
            <>
              <Property label="Expires In" tooltip={notification.expires}>
                <Typography noWrap>
                  {FormatTimeUntil(notification.expires, false)}
                </Typography>
              </Property>

              <Property label="Source">
                <Typography variant="overline">
                  {notification.source}
                </Typography>
              </Property>
            </>
          )}
        </CardContent>
      </CardActionArea>

      <CardActions>
        {!notification.isRead ? (
          <ButtonWithTooltip tooltip="Mark as Read" startIcon={<CheckIcon />}>
            Read
          </ButtonWithTooltip>
        ) : (
          // read button placeholder
          <Box width="77px" />
        )}
      </CardActions>
    </Card>
  );
};
