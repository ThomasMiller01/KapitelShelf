import { type BooksApi, type FileInfoDTO, LocationTypeDTO } from "../../../lib/api/KapitelShelf.Api";

type BookFileClient = Pick<BooksApi, "booksBookIdFileGet" | "booksBookIdGet">;
type FileMetadata = Pick<FileInfoDTO, "fileName" | "mimeType" | "sha256">;

interface BookDownload {
  file: File;
  sha256?: string;
}

const toSha256 = (value: unknown): string | undefined =>
  typeof value === "string" && /^[0-9a-f]{64}$/i.test(value)
    ? value.toLowerCase()
    : undefined;

const download = async (
  client: BookFileClient,
  bookId: string,
  metadata: FileMetadata,
  signal: AbortSignal,
): Promise<BookDownload> => {
  signal.throwIfAborted();
  const response = await client.booksBookIdFileGet(bookId, true, {
    signal,
    responseType: "blob",
    headers: { "Cache-Control": "no-cache" },
  });
  const blob: unknown = response.data;
  if (!(blob instanceof Blob) || blob.size === 0) {
    throw new Error("The stored book file is empty or could not be downloaded.");
  }

  return {
    file: new File([blob], metadata.fileName || "Untitled", {
      type: blob.type || metadata.mimeType || "",
    }),
    sha256: toSha256(response.headers["x-book-file-sha256"]),
  };
};

export const loadBookFile = async (
  client: BookFileClient,
  bookId: string,
  metadata: FileMetadata,
  signal: AbortSignal,
): Promise<BookDownload> => {
  const initial = await download(client, bookId, metadata, signal);
  const expected = toSha256(metadata.sha256);
  if (!initial.sha256 || !expected || initial.sha256 === expected) {
    return initial;
  }

  const { data: book } = await client.booksBookIdGet(bookId, undefined, { signal });
  const current = book.location?.fileInfo;
  if (book.location?.type !== LocationTypeDTO.NUMBER_1 || !current) {
    throw new Error("This book no longer has a stored file.");
  }

  const retried = await download(client, bookId, current, signal);
  const refreshed = toSha256(current.sha256);
  return refreshed && retried.sha256 !== refreshed
    ? { file: retried.file }
    : retried;
};
