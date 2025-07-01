import { Card, CardActionArea, Typography } from "@mui/material";
import CardMedia from "@mui/material/CardMedia";
import { type ReactElement } from "react";

import TestImg from "../assets/test.png";
import type { UserDTO } from "../lib/api/KapitelShelf.Api/api";
import { GetUserColor } from "../utils/UserProfile";

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
        bgcolor: GetUserColor(userProfile.username),
      }}
      onClick={
        onClick !== undefined ? (): void => onClick(userProfile) : undefined
      }
    >
      <CardMedia
        component="img"
        sx={{ maxHeight: "200px", minHeight: "170px" }}
        image={TestImg}
        alt={userProfile.username ?? "User Avatar"}
      />
      <Typography variant="h6" noWrap my="5px">
        {userProfile.username}
      </Typography>
    </CardActionArea>
  </Card>
);

export default UserProfileCard;
