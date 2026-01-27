import { AuthorDTO } from "../lib/api/KapitelShelf.Api";

export const normalizeAuthor = (a: AuthorDTO) => {
  const firstName = a.firstName?.trim() ?? "";
  const lastName = a.lastName?.trim() ?? "";

  return {
    firstName,
    lastName,
  };
};
