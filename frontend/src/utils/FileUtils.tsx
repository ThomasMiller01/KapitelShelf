import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";

export const UrlToFile = async (url: string): Promise<File> => {
  const fileName = url.split("/").pop() ?? "unknown";

  const res = await fetch(url);
  const blob = await res.blob();

  return new File([blob], fileName, { type: blob.type });
};

export const CoverUrl = (book: BookDTO | undefined): string | undefined => {
  if (book === undefined || book?.cover === undefined) {
    return undefined;
  }

  return `${import.meta.env.VITE_KAPITELSHELF_API}/books/${book.id}/cover`;
};
