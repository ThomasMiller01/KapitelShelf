import MonsieurRead from "../assets/users/images/MonsieurRead.png";
import Readini from "../assets/users/images/Readini.png";
import { ProfileImageTypeDTO } from "../lib/api/KapitelShelf.Api/api";
import { UserProfileColors } from "../styles/Palette";

export const GetUserColor = (username: string | undefined | null): string => {
  console.log("default");
  if (username === undefined || username === null) {
    console.log(UserProfileColors[0]);
    return UserProfileColors[0];
  }

  let hash = 0;
  for (let i = 0; i < username.length; i++) {
    hash = username.charCodeAt(i) + ((hash << 5) - hash);
  }
  const idx = Math.abs(hash) % UserProfileColors.length;
  return UserProfileColors[idx];
};

export const ProfileImageTypeToSrc = {
  [ProfileImageTypeDTO.NUMBER_0]: MonsieurRead,
  [ProfileImageTypeDTO.NUMBER_1]: Readini,
};

export const ProfileImageTypeToName = {
  [ProfileImageTypeDTO.NUMBER_0]: "Monsieur Read",
  [ProfileImageTypeDTO.NUMBER_1]: "Readini",
};
