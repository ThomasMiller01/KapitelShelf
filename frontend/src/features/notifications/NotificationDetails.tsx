import { Box, Typography } from "@mui/material";
import type { NotificationDto } from "../../lib/api/KapitelShelf.Api/api";

interface NotificationDetailsProps {
  notification: NotificationDto;
}

const NotificationDetails: React.FC<NotificationDetailsProps> = ({
  notification,
}) => {
  return (
    <Box p={3}>
      <Typography variant="h5" gutterBottom sx={{ wordBreak: "break-word" }}>
        {notification.title}
      </Typography>

      <Typography
        variant="body1"
        color="text.secondary"
        mb="15px"
        sx={{ wordBreak: "break-word" }}
      >
        {notification.message}
      </Typography>
    </Box>
  );
};

export default NotificationDetails;
