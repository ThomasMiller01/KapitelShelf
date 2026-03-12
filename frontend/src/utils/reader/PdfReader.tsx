import { FileInfoDTO } from "../../lib/api/KapitelShelf.Api";
import { BookContent, BookSection, BookTocItem } from "./BookContentModels";

interface PdfTextItem {
  str: string;
  transform: number[];
}

interface PdfTextContent {
  items: unknown[];
}

interface PdfPageProxy {
  getTextContent: () => Promise<PdfTextContent>;
}

interface PdfMetadata {
  info?: Record<string, unknown>;
}

interface PdfOutlineItem {
  title?: string;
  dest: string | unknown[] | null;
  items?: PdfOutlineItem[];
}

interface PdfDocumentProxy {
  numPages: number;
  getMetadata: () => Promise<PdfMetadata>;
  getPage: (pageNumber: number) => Promise<PdfPageProxy>;
  getOutline: () => Promise<PdfOutlineItem[] | null>;
  getDestination: (destination: string) => Promise<unknown[] | null>;
  getPageIndex: (reference: unknown) => Promise<number>;
  destroy: () => Promise<void>;
}

interface PdfJsLib {
  GlobalWorkerOptions: {
    workerSrc: string;
  };
  getDocument: (source: { data: ArrayBuffer }) => {
    promise: Promise<PdfDocumentProxy>;
  };
}

interface GlobalWithPdfJsLib {
  pdfjsLib?: PdfJsLib;
}

let pdfJsLibPromise: Promise<PdfJsLib> | undefined;
let isPdfWorkerConfigured = false;

const loadPdfJsLib = async (): Promise<PdfJsLib> => {
  if (pdfJsLibPromise) {
    return pdfJsLibPromise;
  }

  pdfJsLibPromise = (async () => {
    // @ts-expect-error foliate-js does not ship declarations for vendored PDF.js.
    await import("../../../node_modules/foliate-js/vendor/pdfjs/pdf.mjs");
    const pdfjsLib = (globalThis as GlobalWithPdfJsLib).pdfjsLib;

    if (
      !pdfjsLib ||
      typeof pdfjsLib.getDocument !== "function" ||
      !pdfjsLib.GlobalWorkerOptions
    ) {
      throw new Error("Failed to initialize foliate-js PDF support.");
    }

    if (!isPdfWorkerConfigured) {
      pdfjsLib.GlobalWorkerOptions.workerSrc = new URL(
        "../../../node_modules/foliate-js/vendor/pdfjs/pdf.worker.mjs",
        import.meta.url,
      ).toString();
      isPdfWorkerConfigured = true;
    }

    return pdfjsLib;
  })();

  return pdfJsLibPromise;
};

// Shown when a page has no extractable text.
const EMPTY_PAGE_HTML =
  '<p style="color:#888;text-align:center;padding:2em 0;">No text content on this page.</p>';

const NO_TEXT_PDF_HTML =
  '<p style="color:#888;text-align:center;padding:2em 0;">No extractable text found in this PDF.</p>';

export const ParsePdf = async (
  file: File | undefined,
  fileInfo: FileInfoDTO | undefined,
): Promise<BookContent | undefined> => {
  if (file === undefined) {
    return undefined;
  }

  const pdfjsLib = await loadPdfJsLib();
  const arrayBuffer = await file.arrayBuffer();
  const doc = await pdfjsLib.getDocument({ data: arrayBuffer }).promise;

  try {
    // --- Metadata ---
    let title: string | undefined;
    try {
      const meta = await doc.getMetadata();
      const info = meta.info;
      if (typeof info?.Title === "string" && info.Title.trim().length > 0) {
        title = info.Title.trim();
      }
    } catch {
      // metadata unavailable - fall back to filename
    }
    if (!title) {
      title = fileInfo?.fileName?.replace(/\.[^.]+$/, "") ?? "Untitled";
    }

    // --- Sections (one per page) ---
    const pageCount = doc.numPages;
    const sections: BookSection[] = [];

    for (let pageNum = 1; pageNum <= pageCount; pageNum++) {
      const page = await doc.getPage(pageNum);
      const textContent = await page.getTextContent();

      // Join items into lines using y-coordinate changes as line breaks.
      let lines = "";
      let currentLine = "";
      let lastY: number | null = null;

      for (const item of textContent.items) {
        if (!isPdfTextItem(item)) {
          continue;
        }
        const y = item.transform[5];

        if (lastY !== null && Math.abs(y - lastY) > 1) {
          if (currentLine.trim().length > 0) {
            lines += currentLine.trim() + "\n";
          }
          currentLine = item.str;
        } else {
          currentLine += item.str;
        }
        lastY = y;
      }
      if (currentLine.trim().length > 0) {
        lines += currentLine.trim();
      }

      const text = lines.trim();
      const html = text.length > 0 ? textToHtml(text) : EMPTY_PAGE_HTML;

      const sizeBytes = new TextEncoder().encode(html).byteLength;
      sections.push({
        id: `page-${pageNum}`,
        index: pageNum - 1,
        title: undefined,
        contentType: "pdf-page",
        content: html,
        text,
        sizeBytes,
      });
    }

    // --- TOC (PDF outline) ---
    const outline = await doc.getOutline();
    const toc = outline ? await mapOutlineItems(outline, doc) : [];

    // Remove empty (image-only / no-text) pages and re-index the rest.
    const filteredSections = sections
      .filter((section) => section.content !== EMPTY_PAGE_HTML)
      .map((section, index) => ({ ...section, index }));

    if (filteredSections.length === 0) {
      const fallbackSection: BookSection = {
        id: "pdf-no-text",
        index: 0,
        title: undefined,
        contentType: "pdf-page",
        content: NO_TEXT_PDF_HTML,
        text: undefined,
        sizeBytes: new TextEncoder().encode(NO_TEXT_PDF_HTML).byteLength,
      };

      return {
        metadata: { title },
        navigation: { tableOfContents: [], pageCount: 1 },
        sections: [fallbackSection],
      };
    }

    // Build a map from original PDF page index -> new filtered section index
    // so TOC entries can be remapped correctly.
    const originalToFiltered = new Map<number, number>();
    filteredSections.forEach((section) => {
      const originalIndex = parseInt(section.id.replace("page-", ""), 10) - 1;
      originalToFiltered.set(originalIndex, section.index);
    });

    const remappedToc = remapTocIndices(toc, originalToFiltered);

    return {
      metadata: { title },
      navigation: {
        tableOfContents: remappedToc,
        pageCount: filteredSections.length,
      },
      sections: filteredSections,
    };
  } finally {
    await doc.destroy();
  }
};

const isPdfTextItem = (value: unknown): value is PdfTextItem => {
  if (typeof value !== "object" || value === null) {
    return false;
  }

  const candidate = value as Partial<PdfTextItem>;
  return typeof candidate.str === "string" && Array.isArray(candidate.transform);
};

const escapeHtml = (text: string): string =>
  text
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");

// Wrap extracted plain text in HTML paragraphs.
// Single newlines become <br>, double newlines start a new <p>.
const textToHtml = (text: string): string =>
  text
    .split(/\n{2,}/)
    .map((block) => `<p>${escapeHtml(block).replace(/\n/g, "<br>")}</p>`)
    .join("\n");

const remapTocIndices = (
  items: BookTocItem[],
  map: Map<number, number>,
): BookTocItem[] =>
  items.map((item) => ({
    ...item,
    sectionIndex:
      item.sectionIndex !== undefined ? map.get(item.sectionIndex) : undefined,
    children: remapTocIndices(item.children, map),
  }));

// Recursively map PDF outline items to BookTocItem[].
// Resolves named or explicit destinations to a page index (0-based section index).
const mapOutlineItems = async (
  items: PdfOutlineItem[],
  doc: PdfDocumentProxy,
  counter = { value: 0 },
): Promise<BookTocItem[]> => {
  const result: BookTocItem[] = [];

  for (const item of items) {
    const id = `toc-${counter.value++}`;
    let sectionIndex: number | undefined;

    try {
      let dest = item.dest;
      if (typeof dest === "string") {
        dest = await doc.getDestination(dest);
      }
      if (Array.isArray(dest) && dest.length > 0) {
        const pageIndex = await doc.getPageIndex(dest[0]);
        sectionIndex = pageIndex; // 0-based, matches section index (page-1 has index 0)
      }
    } catch {
      // destination unresolvable — leave sectionIndex undefined
    }

    const children = item.items && item.items.length > 0
      ? await mapOutlineItems(item.items, doc, counter)
      : [];

    result.push({
      id,
      label: item.title ?? "Untitled",
      sectionIndex,
      children,
    });
  }

  return result;
};
