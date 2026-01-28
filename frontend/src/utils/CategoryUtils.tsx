import { CategoryDTO } from "../lib/api/KapitelShelf.Api";

export const normalizeCategory = (c: CategoryDTO) => {
  const name = c.name?.trim() ?? "";

  return {
    name,
  };
};
