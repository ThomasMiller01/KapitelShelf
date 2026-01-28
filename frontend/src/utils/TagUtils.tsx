import { TagDTO } from "../lib/api/KapitelShelf.Api";

export const normalizeTag = (t: TagDTO) => {
  const name = t.name?.trim() ?? "";

  return {
    name,
  };
};
