import CheeryChino from "../assets/users/CheeryChino.png";
import Freshavocado from "../assets/users/Freshavocado.png";
import MonsieurRead from "../assets/users/MonsieurRead.png";
import MuggyMalheur from "../assets/users/MuggyMalheur.png";
import Readini from "../assets/users/Readini.png";
import Riley from "../assets/users/Riley.png";
import SunnyTome from "../assets/users/SunnyTome.png";
import Tailywink from "../assets/users/Tailywink.png";
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
  [ProfileImageTypeDTO.NUMBER_2]: MuggyMalheur,
  [ProfileImageTypeDTO.NUMBER_3]: CheeryChino,
  [ProfileImageTypeDTO.NUMBER_4]: SunnyTome,
  [ProfileImageTypeDTO.NUMBER_5]: Freshavocado,
  [ProfileImageTypeDTO.NUMBER_6]: Tailywink,
  [ProfileImageTypeDTO.NUMBER_7]: Riley,
};

export const ProfileImageTypeToName = {
  [ProfileImageTypeDTO.NUMBER_0]: "Monsieur Read",
  [ProfileImageTypeDTO.NUMBER_1]: "Readini",
  [ProfileImageTypeDTO.NUMBER_2]: "Muggy Malheur",
  [ProfileImageTypeDTO.NUMBER_3]: "Cheery Chino",
  [ProfileImageTypeDTO.NUMBER_4]: "Sunny Tome",
  [ProfileImageTypeDTO.NUMBER_5]: "FR E SH A VOCA DO",
  [ProfileImageTypeDTO.NUMBER_6]: "Tailywink",
  [ProfileImageTypeDTO.NUMBER_7]: "Riley",
};

export const ProfileImageCategories = {
  "Stuff with Faces": [
    ProfileImageTypeDTO.NUMBER_0,
    ProfileImageTypeDTO.NUMBER_2,
    ProfileImageTypeDTO.NUMBER_3,
    ProfileImageTypeDTO.NUMBER_4,
    ProfileImageTypeDTO.NUMBER_5,
  ],
  "Quirky Companions": [
    ProfileImageTypeDTO.NUMBER_1,
    ProfileImageTypeDTO.NUMBER_6,
  ],
  Chicks: [ProfileImageTypeDTO.NUMBER_7],
};

export const UserProfileColors = [
  "#39796b", // teal
  "#b26a22", // orange
  "#1976d2", // blue
  "#c4373c", // red
  "#2997a3", // cyan
  "#a15033", // deep orange
  "#388e3c", // green
  "#b2a429", // gold
  "#7b1fa2", // purple
  "#879623", // olive green
];
