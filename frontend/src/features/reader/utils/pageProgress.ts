export const mapPage = (page: number, fromPages: number, toPages: number): number => {
  if (fromPages <= 1 || toPages <= 1) {
    return 0;
  }

  return Math.round(
    (Math.min(Math.max(page, 0), fromPages - 1) * (toPages - 1)) /
      (fromPages - 1),
  );
};
