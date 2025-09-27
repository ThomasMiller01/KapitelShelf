import EditIcon from "@mui/icons-material/Edit";
import { Card, CardActionArea } from "@mui/material";
import CardMedia from "@mui/material/CardMedia";
import { type ReactElement } from "react";
import { Link } from "react-router-dom";

import {
  ProfileImageTypeDTO,
  type UserDTO,
} from "../lib/api/KapitelShelf.Api/api";
import { ProfileImageTypeToSrc } from "../utils/UserProfileUtils";
import { IconButtonWithTooltip } from "./base/IconButtonWithTooltip";
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
        bgcolor: userProfile?.color,
        position: "relative",
      }}
      onClick={
        onClick !== undefined ? (): void => onClick(userProfile) : undefined
      }
    >
      <CardMedia
        component="img"
        sx={{
          height: "auto",
          width: "100%",
          aspectRatio: "1 / 1",
        }}
        image={
          ProfileImageTypeToSrc[
            userProfile.image ?? ProfileImageTypeDTO.NUMBER_0
          ]
        }
        alt={userProfile.username ?? "User Avatar"}
      />
      <FancyText variant="h6" noWrap mb="5px">
        {userProfile.username}
      </FancyText>
      <IconButtonWithTooltip
        tooltip="Edit Profile"
        component={Link}
        to="/profile/edit"
        sx={{
          position: "absolute",
          top: 5,
          right: 5,
        }}
      >
        <EditIcon fontSize="inherit" />
      </IconButtonWithTooltip>
    </CardActionArea>
  </Card>
);

export default UserProfileCard;
