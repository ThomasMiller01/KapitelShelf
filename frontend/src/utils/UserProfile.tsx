import { UserProfileColors } from "../styles/Palette";

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
