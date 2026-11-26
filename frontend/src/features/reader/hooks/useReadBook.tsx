import { useEffect, useState } from "react";

import { type BookDTO, LocationTypeDTO } from "../../../lib/api/KapitelShelf.Api";
import { useApi } from "../../../shared/contexts/ApiProvider";
import type { BookContent } from "../utils/BookContentModels";
import { parseBookFile } from "../utils/BookParser";

interface LoadResult {
  key: string;
  content?: BookContent;
  error?: string;
}

interface ReadBookResult {
  content?: BookContent;
  error?: string;
  isLoading: boolean;
  retry: () => void;
}

export const useReadBook = (book: BookDTO): ReadBookResult => {
  const { basePath, clients: { books } } = useApi();
  const [attempt, setAttempt] = useState(0);
  const [result, setResult] = useState<LoadResult>();
  const { id: bookId, location } = book;
  const info = location?.fileInfo;
  const hasLocalFile = location?.type === LocationTypeDTO.NUMBER_1 && Boolean(info);
  const { fileName, mimeType, sha256 } = info ?? {};
  const key = JSON.stringify([basePath, bookId, hasLocalFile, info?.id, info?.filePath, fileName, mimeType, sha256, attempt]);

  useEffect(() => {
    setResult(undefined);
    if (!bookId || !hasLocalFile) {
      return;
    }

    const controller = new AbortController();
    const { signal } = controller;
    let release: (() => void) | undefined;

    const read = async (): Promise<void> => {
      try {
        const { data } = await books.booksBookIdFileGet(bookId, { signal, responseType: "blob" });
        const blob: unknown = data;
        if (!(blob instanceof Blob) || blob.size === 0) {
          throw new Error("The stored book file is empty or could not be downloaded.");
        }
        const file = new File([blob], fileName || "Untitled", { type: blob.type || mimeType || "" });
        const content = await parseBookFile(file, signal);
        if (signal.aborted) {
          content.release?.();
          return;
        }
        ({ release } = content);
        setResult({ key, content });
      } catch (error) {
        if (!signal.aborted) {
          setResult({ key, error: error instanceof Error ? error.message : "The book could not be opened." });
        }
      }
    };

    void read();
    return (): void => {
      controller.abort();
      release?.();
    };
  }, [bookId, books, fileName, hasLocalFile, key, mimeType]);

  const current = result?.key === key ? result : undefined;
  const canLoad = Boolean(bookId) && hasLocalFile;
  return {
    content: current?.content,
    error: canLoad ? current?.error : "This book does not have a stored file.",
    isLoading: canLoad && !current,
    retry: (): void => setAttempt((count) => count + 1),
  };
};
