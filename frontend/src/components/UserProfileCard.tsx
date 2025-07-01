import PersonIcon from "@mui/icons-material/Person";
import { Avatar, Card, CardActionArea, Typography } from "@mui/material";
import { deepPurple } from "@mui/material/colors";
import { type ReactElement } from "react";

import type { UserDTO } from "../lib/api/KapitelShelf.Api/api";

const getInitials = (username: string): string =>
  username
    .split(" ")
    .map((n) => n[0]?.toUpperCase() ?? "")
    .join("")
    .slice(0, 2);

interface UserProfileCardProps {
  userProfile: UserDTO;
  onClick?: (userProfile: UserDTO) => void;
}

const UserProfileCard = ({
  userProfile,
  onClick,
}: UserProfileCardProps): ReactElement => (
  <Card elevation={4} sx={{ borderRadius: 4 }}>
    <CardActionArea
      sx={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        py: 3,
        px: 1,
      }}
      onClick={
        onClick !== undefined ? (): void => onClick(userProfile) : undefined
      }
    >
      <Avatar
        sx={{
          width: 72,
          height: 72,
          bgcolor: deepPurple[400],
          mb: 2,
          fontSize: 32,
        }}
      >
        {getInitials(userProfile.username ?? "") || (
          <PersonIcon fontSize="large" />
        )}
      </Avatar>
      <Typography variant="subtitle1" noWrap>
        {userProfile.username}
      </Typography>
    </CardActionArea>
  </Card>
);

export default UserProfileCard;
