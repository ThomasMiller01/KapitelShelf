import MonsieurRead from "../assets/users/images/MonsieurRead.png";
import Readini from "../assets/users/images/Readini.png";
import { ProfileImageTypeDTO } from "../lib/api/KapitelShelf.Api/api";

export const GetUserColor = (username: string | undefined | null): string => {
  if (username === undefined || username === null) {
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

export const UserProfileColors = [
  "#39796b", // teal
  "#b26a22", // orange
  "#1976d2", // blue
  "#b11218", // red
  "#2997a3", // cyan
  "#a15033", // deep orange
  "#388e3c", // green
  "#b2a429", // gold
  "#7b1fa2", // purple
  "#879623", // olive green
];
