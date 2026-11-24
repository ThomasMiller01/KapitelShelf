import type { BookContent } from "./BookContentModels";
import { ParseEpub } from "./EpubReader";
import { ParseFb2 } from "./Fb2Reader";
import { ParsePdf } from "./PdfReader";
import { ParseTxt } from "./TxtReader";

type BookParser = (file: File, signal: AbortSignal) => Promise<BookContent>;

const formats = [
  { extension: ".epub", mimeTypes: ["application/epub+zip"], parse: ParseEpub },
  { extension: ".fb2", mimeTypes: ["application/x-fictionbook+xml", "application/x-fb2"], parse: ParseFb2 },
  { extension: ".pdf", mimeTypes: ["application/pdf"], parse: ParsePdf },
  { extension: ".txt", mimeTypes: ["text/plain"], parse: ParseTxt },
] satisfies { extension: string; mimeTypes: string[]; parse: BookParser }[];

export const parseBookFile = async (file: File, signal: AbortSignal): Promise<BookContent> => {
  signal.throwIfAborted();
  const name = file.name.toLowerCase();
  const mimeType = file.type.split(";", 1)[0].trim().toLowerCase();
  const format = formats.find(({ extension }) => name.endsWith(extension))
    ?? formats.find(({ mimeTypes }) => mimeTypes.includes(mimeType));
  if (!format) {
    throw new Error("This reader supports EPUB, FB2, PDF, and TXT files.");
  }

  const content = await format.parse(file, signal);
  if (signal.aborted || content.sections.length === 0) {
    content.release?.();
    signal.throwIfAborted();
    throw new Error("The book does not contain any readable sections.");
  }
  return content;
};
