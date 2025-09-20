import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";
import { GetMobileApiBaseUrl, IsMobileApp } from "./MobileUtils";

export const UrlToFile = async (url: string): Promise<File> => {
  const fileName = url.split("/").pop() ?? "unknown";

  const res = await fetch(url);
  const blob = await res.blob();

  return new File([blob], fileName, { type: blob.type });
};

export const CoverUrl = (book: BookDTO | undefined): string | undefined => {
  if (book === undefined || book?.cover === undefined || book?.cover === null) {
    return undefined;
  }

  const APIUrl = IsMobileApp()
    ? GetMobileApiBaseUrl()
    : import.meta.env.VITE_KAPITELSHELF_API;
  return `${APIUrl}/books/${book.id}/cover`;
};

export const BookFileUrl = (book: BookDTO | undefined): string | undefined => {
  if (
    book === undefined ||
    book?.location?.type !== 1 ||
    book?.location?.fileInfo === undefined ||
    book?.location?.fileInfo === null
  ) {
    return undefined;
  }

  const APIUrl = IsMobileApp()
    ? GetMobileApiBaseUrl()
    : import.meta.env.VITE_KAPITELSHELF_API;
  return `${APIUrl}/books/${book.id}/file`;
};

export const RenameFile = (file: File, fileName: string): File =>
  new File(
    [file], // keep the same file‐contents
    fileName, // override the name
    {
      type: file.type, // preserve MIME type
      lastModified: file.lastModified, // preserve last-modified timestamp
    }
  );
