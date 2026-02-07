import { Card, CardContent, Rating, Stack, Typography } from "@mui/material";
import { UserBookMetadataDTO } from "../lib/api/KapitelShelf.Api";
import { FormatTimeUntil } from "../utils/TimeUtils";

interface UserBookMetadataProps {
  userMetadata: UserBookMetadataDTO;
  isCurrentUser: boolean;
}

export const UserBookMetadata: React.FC<UserBookMetadataProps> = ({
  userMetadata,
  isCurrentUser,
}) => {
  return (
    <Card
      sx={{
        border: isCurrentUser ? "1px solid" : "none",
        borderColor: "secondary.main",
      }}
    >
      <CardContent
        sx={{
          pt: "15px !important",
          pb: "15px !important",
        }}
      >
        <Stack
          direction="row"
          spacing={2}
          alignItems="center"
          mb={userMetadata.notes ? 1.5 : 0}
        >
          <Typography
            gutterBottom
            sx={{ color: "text.secondary", fontSize: 14 }}
          >
            {userMetadata.user?.username}
          </Typography>
          {userMetadata.rating && (
            <Rating
              value={userMetadata.rating / 2}
              defaultValue={0}
              max={5}
              precision={0.5}
              readOnly
              size="small"
            />
          )}
        </Stack>
        {userMetadata.notes && (
          <Typography variant="body2">{userMetadata.notes}</Typography>
        )}
        <Typography sx={{ color: "text.secondary", mt: 1, fontSize: 12 }}>
          {FormatTimeUntil(userMetadata.createdOn, true)}
        </Typography>
      </CardContent>
    </Card>
  );
};
