import CheckIcon from "@mui/icons-material/Check";
import {
  Box,
  Card,
  CardActionArea,
  CardActions,
  CardContent,
  Stack,
  Tooltip,
  Typography,
} from "@mui/material";

import { useMobile } from "../../hooks/useMobile";
import { type NotificationDto } from "../../lib/api/KapitelShelf.Api";
import {
  NotificationSeverityColor,
  NotificationSeverityToString,
  NotificationTypeIcon,
  NotificationTypeToString,
} from "../../utils/NotificationUtils";
import { ButtonWithTooltip } from "../base/ButtonWithTooltip";

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
      }}
    >
      <CardActionArea
        sx={{
          display: "flex",
          justifyContent: "space-between",
          paddingRight: isMobile ? "0" : "10px",
        }}
      >
        <CardContent sx={{ padding: "6px 10px !important" }}>
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
        </CardContent>
        {!isMobile && (
          <Box>
            <Typography variant="overline">{notification.source}</Typography>
          </Box>
        )}
      </CardActionArea>
      <CardActions>
        <ButtonWithTooltip tooltip="Mark as Read" startIcon={<CheckIcon />}>
          Read
        </ButtonWithTooltip>
      </CardActions>
    </Card>
  );
};
