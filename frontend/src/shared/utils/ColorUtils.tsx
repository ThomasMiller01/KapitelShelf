import { Theme } from "@mui/material";

export const GetColor = (
  color: string | undefined,
  theme: Theme
): string | unknown => {
  if (color?.includes(".")) {
    // try extract color from theme
    return color
      .split(".")
      .reduce<unknown>(
        (acc, key) => (acc as Record<string, unknown>)?.[key],
        theme.palette
      );
  }

  // normal color code
  return color;
};
