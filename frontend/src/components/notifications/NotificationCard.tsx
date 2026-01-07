import CheckIcon from "@mui/icons-material/Check";
import {
  Badge,
  BadgeProps,
  Box,
  Card,
  CardActions,
  CardContent,
  Container,
  Stack,
  styled,
  Tooltip,
  Typography,
} from "@mui/material";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApi } from "../../contexts/ApiProvider";
import { useMobile } from "../../hooks/useMobile";
import { useUserProfile } from "../../hooks/useUserProfile";
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
import { ActionWrapper } from "../layout/ItemCard/ItemCardLayout";

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

export interface NotificationCardProps {
  notification: NotificationDto;
  hideChildCount?: boolean;
  hideReadStatus?: boolean;
  disableLink?: boolean;
  hideActions?: boolean;
  showDetails?: boolean;
}

export const NotificationCard: React.FC<NotificationCardProps> = ({
  notification,
  hideChildCount = false,
  hideReadStatus = false,
  disableLink = false,
  hideActions = false,
  showDetails = false,
}) => {
  const { isMobile } = useMobile();
  const { profile } = useUserProfile();
  const { clients } = useApi();
  const queryClient = useQueryClient();

  const { mutate: markAsRead } = useMutation({
    mutationFn: async () => {
      if (profile?.id === undefined || notification.id === undefined) {
        return;
      }

      await clients.notifications.notificationsIdReadPost(
        notification.id,
        profile.id
      );
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: ["notifications-list", profile?.id],
        }),
        queryClient.invalidateQueries({
          queryKey: ["notifications-stats", profile?.id],
        }),
      ]);
    },
    meta: {
      notify: {
        enabled: true,
        operation: `Marking notification as read`,
        showLoading: true,
        showSuccess: false,
        showError: true,
      },
    },
  });

  return (
    <Card
      variant="outlined"
      sx={{
        display: "flex",
        borderLeft: `5px solid`,
        borderLeftColor: NotificationSeverityColor(notification),
        opacity: !hideReadStatus && notification.isRead ? 0.6 : 1,
      }}
    >
      <ActionWrapper
        link={disableLink ? undefined : `/notifications/${notification.id}`}
        sx={{
          paddingRight: isMobile ? "0" : "10px",
          width: "100%",
        }}
      >
        <CardContent
          sx={{
            padding: "6px 10px !important",
            display: "flex",
            gap: "15px",
            justifyContent: {
              xs: "space-evenly",
              lg: "space-between",
            },
            flexWrap: "wrap",
          }}
        >
          <Container disableGutters maxWidth="sm" sx={{ m: 0 }}>
            <TitleBadge
              badgeContent={hideChildCount ? 0 : notification.children?.length}
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

          {showDetails && (
            <>
              <Property label="Created">
                <Stack direction="row" spacing={0.5} alignItems="end">
                  <Typography noWrap>
                    {FormatTimeUntil(notification.created, true, "time")}
                  </Typography>
                  <Typography variant="body2" color="secondary" noWrap>
                    {FormatTimeUntil(notification.created, true, "date")}
                  </Typography>
                </Stack>
              </Property>

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
      </ActionWrapper>

      {!hideActions && (
        <CardActions
          sx={{
            p: {
              xs: 0,
              sm: 1,
            },
          }}
        >
          {!notification.isRead ? (
            <ButtonWithTooltip
              tooltip="Mark as Read"
              startIcon={<CheckIcon />}
              onClick={() => markAsRead()}
            >
              Read
            </ButtonWithTooltip>
          ) : (
            // read button placeholder
            <Box width="77px" />
          )}
        </CardActions>
      )}
    </Card>
  );
};
