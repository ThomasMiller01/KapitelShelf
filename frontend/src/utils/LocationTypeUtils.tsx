/* eslint-disable no-magic-numbers */

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
