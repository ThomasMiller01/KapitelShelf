import { useEffect, useState } from "react";
import { BookDTO, FileInfoDTO } from "../lib/api/KapitelShelf.Api";
import { BookFileUrl, UrlToFile } from "../utils/FileUtils";
import { BookContent } from "../utils/reader/BookContentModels";
import { ParseEpub } from "../utils/reader/EpubReader";
import { ParsePdf } from "../utils/reader/PdfReader";
import { ParseTxt } from "../utils/reader/TxtReader";

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

  console.log("Parsed book content:", parsed);

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

  // EPUB
  if (
    fileInfo?.mimeType === "application/epub+zip" ||
    fileInfo?.fileName?.endsWith(".epub")
  ) {
    return await ParseEpub(file, fileInfo);
  }

  // TXT
  if (
    fileInfo?.mimeType === "text/plain" ||
    fileInfo?.fileName?.endsWith(".txt")
  ) {
    return await ParseTxt(file, fileInfo);
  }

  // PDF
  if (
    fileInfo?.mimeType === "application/pdf" ||
    fileInfo?.fileName?.endsWith(".pdf")
  ) {
    return await ParsePdf(file, fileInfo);
  }

  throw new Error(
    `Unsupported book MIME type '${
      fileInfo?.mimeType
    }' and file ending: '.${fileInfo?.fileName?.split(".").pop()}'`,
  );
};
