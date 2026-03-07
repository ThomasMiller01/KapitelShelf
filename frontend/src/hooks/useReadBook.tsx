import { useEffect, useState } from "react";
import { BookDTO, FileInfoDTO } from "../lib/api/KapitelShelf.Api";
import { BookFileUrl, UrlToFile } from "../utils/FileUtils";
import { BookContent } from "../utils/bookReader/BookContent";
import { ParseEpub } from "../utils/bookReader/epubReader";

interface useReadBookResult {
  content: BookContent | undefined;
  isLoading: boolean;
  error: string | undefined;
}

export const useReadBook = (book: BookDTO): useReadBookResult => {
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | undefined>(undefined);

  const [file, setFile] = useState<File | undefined>(undefined);
  useEffect(() => {
    LoadBookFile(book)
      .then((f) => setFile(f))
      .catch((e) => {
        setError(e.message);
        setIsLoading(false);
      });
  }, [book]);

  const [parsed, setParsed] = useState<any>(undefined);
  useEffect(() => {
    if (file === undefined) {
      return;
    }

    ParseBook(file, book.location?.fileInfo)
      .then((p) => setParsed(p))
      .catch((e) => setError(e.message))
      .finally(() => setIsLoading(false));
  }, [file, isLoading]);

  return {
    content: parsed,
    isLoading,
    error,
  };
};

const LoadBookFile = async (book: BookDTO): Promise<File | undefined> => {
  const fileUrl = BookFileUrl(book);
  if (fileUrl === undefined) {
    throw new Error("This book does not have a stored file.");
  }

  return await UrlToFile(fileUrl);
};

const ParseBook = async (
  file: File | undefined,
  fileInfo: FileInfoDTO | undefined,
): Promise<BookContent | undefined> => {
  if (file === undefined) {
    throw new Error("The file of this book could not be loaded.");
  }

  if (fileInfo?.fileName?.endsWith(".epub")) {
    return await ParseEpub(file, fileInfo);
  }

  throw new Error(`Unsupported book MIME type: ${fileInfo?.mimeType}`);

  //   switch (fileInfo?.mimeType) {
  //     case "application/epub+zip":
  //       return await ParseEpub(file, fileInfo);

  //     default:
  //       console.warn(`Unsupported book MIME type: ${fileInfo?.mimeType}`);
  //       return;
  //   }
};
