export const toTitleCase = (value: string | undefined) => {
  if (value === undefined) {
    return undefined;
  }

  return value
    .toLowerCase()
    .split(" ")
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");
};
