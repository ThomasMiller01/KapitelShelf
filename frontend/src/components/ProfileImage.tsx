import { Box, Button, Tooltip } from "@mui/material";

import type { ProfileImageTypeDTO } from "../lib/api/KapitelShelf.Api/api";
import {
  ProfileImageTypeToName,
  ProfileImageTypeToSrc,
} from "../utils/UserProfile";

interface ProfileImageProps {
  profileImageType: ProfileImageTypeDTO;
  profileColor: string;

  maxHeight?: number;
  onClick?: (profileImageType: ProfileImageTypeDTO) => void | undefined;
}

export const ProfileImage: React.FC<ProfileImageProps> = ({
  profileImageType,
  profileColor,
  maxHeight = 200,
  onClick,
}) => (
  <Box
    component={onClick !== undefined ? Button : Box}
    onClick={
      onClick !== undefined ? (): void => onClick(profileImageType) : undefined
    }
    sx={{
      bgcolor: profileColor,
      borderRadius: "32px",
      display: "block",
    }}
  >
    <Tooltip title={ProfileImageTypeToName[profileImageType]}>
      <img
        style={{
          minHeight: maxHeight - 30,
          maxHeight,
        }}
        src={ProfileImageTypeToSrc[profileImageType]}
        alt={ProfileImageTypeToName[profileImageType]}
      />
    </Tooltip>
  </Box>
);
