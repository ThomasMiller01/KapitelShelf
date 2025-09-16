import CheeryChino from "../assets/users/CheeryChino.png";
import Chico from "../assets/users/Chico.png";
import Ellie from "../assets/users/Ellie.png";
import Freshavocado from "../assets/users/Freshavocado.png";
import LeChonkyBoy from "../assets/users/LeChonkyBoy.png";
import LittleStinky from "../assets/users/LittleStinky.png";
import MonsieurRead from "../assets/users/MonsieurRead.png";
import MuggyMalheur from "../assets/users/MuggyMalheur.png";
import Readini from "../assets/users/Readini.png";
import Riley from "../assets/users/Riley.png";
import Tailywink from "../assets/users/Tailywink.png";
import type { UserSettingDTO } from "../lib/api/KapitelShelf.Api/api";
import {
  ProfileImageTypeDTO,
  UserSettingValueTypeDTO,
} from "../lib/api/KapitelShelf.Api/api";

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
  [ProfileImageTypeDTO.NUMBER_4]: LittleStinky,
  [ProfileImageTypeDTO.NUMBER_5]: Freshavocado,
  [ProfileImageTypeDTO.NUMBER_6]: Tailywink,
  [ProfileImageTypeDTO.NUMBER_7]: Riley,
  [ProfileImageTypeDTO.NUMBER_8]: Chico,
  [ProfileImageTypeDTO.NUMBER_9]: LeChonkyBoy,
  [ProfileImageTypeDTO.NUMBER_10]: Ellie,
};

export const ProfileImageTypeToName = {
  [ProfileImageTypeDTO.NUMBER_0]: "Monsieur Read",
  [ProfileImageTypeDTO.NUMBER_1]: "Readini",
  [ProfileImageTypeDTO.NUMBER_2]: "Muggy Malheur",
  [ProfileImageTypeDTO.NUMBER_3]: "Cheery Chino",
  [ProfileImageTypeDTO.NUMBER_4]: "Little Stinky",
  [ProfileImageTypeDTO.NUMBER_5]: "FR E SH A VOCA DO",
  [ProfileImageTypeDTO.NUMBER_6]: "Tailywink",
  [ProfileImageTypeDTO.NUMBER_7]: "Riley",
  [ProfileImageTypeDTO.NUMBER_8]: "Chico",
  [ProfileImageTypeDTO.NUMBER_9]: "Le Chonky Boy",
  [ProfileImageTypeDTO.NUMBER_10]: "Ellie",
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
    ProfileImageTypeDTO.NUMBER_9,
  ],
  Chicks: [
    ProfileImageTypeDTO.NUMBER_7,
    ProfileImageTypeDTO.NUMBER_8,
    ProfileImageTypeDTO.NUMBER_10,
  ],
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

export type UserSettingValueType =
  | string
  | number
  | boolean
  | string[]
  | number[];

/** converts a dto to a js primitive */
export const UserSettingValueFromDto = (
  s: UserSettingDTO
): UserSettingValueType => {
  switch (s.type) {
    case UserSettingValueTypeDTO.NUMBER_1:
      return Number(s.value);
    case UserSettingValueTypeDTO.NUMBER_2:
      return s.value === "true";
    case UserSettingValueTypeDTO.NUMBER_3:
      return JSON.parse(s.value ?? "[]");
    case UserSettingValueTypeDTO.NUMBER_4:
      return JSON.parse(s.value ?? "[]");
    default:
      return s.value ?? "";
  }
};

/** converts a js primitive to a dto */
export const UserSettingValueToDto = (
  key: string,
  v: UserSettingValueType
): UserSettingDTO => {
  if (typeof v === "number") {
    return { key, value: String(v), type: UserSettingValueTypeDTO.NUMBER_1 };
  }
  if (typeof v === "boolean") {
    return {
      key,
      value: v ? "true" : "false",
      type: UserSettingValueTypeDTO.NUMBER_2,
    };
  }
  if (Array.isArray(v) && v.every((item) => typeof item === "string")) {
    return {
      key,
      value: JSON.stringify(v),
      type: UserSettingValueTypeDTO.NUMBER_3,
    };
  }
  if (Array.isArray(v) && v.every((item) => typeof item === "number")) {
    return {
      key,
      value: JSON.stringify(v),
      type: UserSettingValueTypeDTO.NUMBER_4,
    };
  }
  return {
    key,
    value: String(v ?? ""),
    type: UserSettingValueTypeDTO.NUMBER_0,
  };
};

/**
 * coerce a union value into the type of sample T
 * @returns coerced value of type T
 */
export const UserSettingValueCoerceToType = <T extends UserSettingValueType>(
  value: UserSettingValueType,
  sample: T
): T => {
  if (Array.isArray(sample)) {
    // Handle string[] or number[]
    if (sample.every((item) => typeof item === "string")) {
      if (Array.isArray(value)) {
        return value.map((v) => String(v)) as T;
      }
      if (typeof value === "string") {
        try {
          const parsed = JSON.parse(value);
          if (Array.isArray(parsed)) {
            return parsed.map((v) => String(v)) as T;
          }
        } catch {
          return [] as unknown as T;
        }
      }
      return [] as unknown as T;
    }

    if (sample.every((item) => typeof item === "number")) {
      if (Array.isArray(value)) {
        return value.map((v) => Number(v)) as T;
      }
      if (typeof value === "string") {
        try {
          const parsed = JSON.parse(value);
          if (Array.isArray(parsed)) {
            return parsed.map((v) => Number(v)) as T;
          }
        } catch {
          return [] as unknown as T;
        }
      }
      return [] as unknown as T;
    }

    // fallback: unknown array type
    return [] as unknown as T;
  }

  // Non-array types
  const t = typeof sample;
  if (t === "string") {
    return String(value) as T;
  }

  if (t === "number") {
    return (typeof value === "number" ? value : Number(value)) as T;
  }

  return (typeof value === "boolean" ? value : value === "true") as T;
};
