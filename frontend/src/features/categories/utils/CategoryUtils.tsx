import { CategoryDTO } from "../../../lib/api/KapitelShelf.Api/index.ts";

export const normalizeCategory = (c: CategoryDTO) => {
  const name = c.name?.trim() ?? "";

  return {
    name,
  };
};
