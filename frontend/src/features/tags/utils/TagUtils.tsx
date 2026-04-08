import { TagDTO } from "../../../lib/api/KapitelShelf.Api/index.ts";

export const normalizeTag = (t: TagDTO) => {
  const name = t.name?.trim() ?? "";

  return {
    name,
  };
};
