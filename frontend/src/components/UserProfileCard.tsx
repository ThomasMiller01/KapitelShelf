import { Card, CardActionArea } from "@mui/material";
import CardMedia from "@mui/material/CardMedia";
import { type ReactElement } from "react";

import WizardProfile from "../assets/Wizard.png";
import type { UserDTO } from "../lib/api/KapitelShelf.Api/api";
import { GetUserColor } from "../utils/UserProfile";
import FancyText from "./FancyText";

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
        sx={{
          minHeight: "170px",
          maxHeight: "200px",
        }}
        image={WizardProfile}
        alt={userProfile.username ?? "User Avatar"}
      />
      <FancyText variant="h6" noWrap my="5px">
        {userProfile.username}
      </FancyText>
    </CardActionArea>
  </Card>
);

export default UserProfileCard;
