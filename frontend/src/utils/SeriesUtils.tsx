import { SeriesDTO } from "../lib/api/KapitelShelf.Api";

export const normalizeSeries = (s: SeriesDTO) => {
  const name = s.name?.trim() ?? "";

  return {
    name,
  };
};
