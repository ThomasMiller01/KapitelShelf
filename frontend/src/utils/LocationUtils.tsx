/* eslint-disable no-magic-numbers */

import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";
import { LocationTypeDTO } from "../lib/api/KapitelShelf.Api/api";

export const RealWorldTypes = [
  0, // Physical
  5, // Library
];

export const LocalTypes = [
  1, // KapitelShelf
];

export const UrlTypes = [
  2, // Kindle
  3, // Skoobe
  4, // Onleihe
];

interface LocationTypeToStringResult {
  [key: number]: string;
}

export const LocationTypeToString: LocationTypeToStringResult = {
  [-1]: "Unavailable",
  0: "Physical",
  1: "KapitelShelf",
  2: "Kindle",
  3: "Skoobe",
  4: "Onleihe",
  5: "Library",
};

// grab the array of valid values once
const allLocationTypes = Object.values(LocationTypeDTO) as LocationTypeDTO[];

/**
 * Type‐guard: returns true if `value` is one of 0,1,2,3,4,5
 */
export function isLocationTypeDTO(value: number): value is LocationTypeDTO {
  return allLocationTypes.includes(value as LocationTypeDTO);
}

/**
 * Convert an arbitrary number to `LocationTypeDTO | undefined`
 */
export function toLocationTypeDTO(value: number): LocationTypeDTO | undefined {
  return isLocationTypeDTO(value) ? (value as LocationTypeDTO) : undefined;
}

export const FileUrl = (book: BookDTO | undefined): string | undefined => {
  if (book === undefined || book?.location?.fileInfo === undefined) {
    return undefined;
  }

  return `${import.meta.env.VITE_KAPITELSHELF_API}/books/${book.id}/file`;
};
