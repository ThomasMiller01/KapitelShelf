import * as pdfjsLib from "pdfjs-dist";
import { FileInfoDTO } from "../../lib/api/KapitelShelf.Api";
import { BookContent, BookSection, BookTocItem } from "./BookContentModels";

// Point the worker at the bundled worker file so pdfjs-dist can offload
// parsing to a background thread without a separate server endpoint.
pdfjsLib.GlobalWorkerOptions.workerSrc = new URL(
  "pdfjs-dist/build/pdf.worker.min.mjs",
  import.meta.url,
).toString();

interface PdfOutlineItem {
  title: string;
  dest: string | unknown[] | null;
  items: PdfOutlineItem[];
}

// Shown when a page has no extractable text (e.g. cover image, full-page illustration).
const EMPTY_PAGE_HTML =
  '<p style="color:#888;text-align:center;padding:2em 0;">No text content on this page.</p>';

export const ParsePdf = async (
  file: File | undefined,
  fileInfo: FileInfoDTO | undefined,
): Promise<BookContent | undefined> => {
  if (file === undefined) {
    return undefined;
  }

  const arrayBuffer = await file.arrayBuffer();
  const doc = await pdfjsLib.getDocument({ data: arrayBuffer }).promise;

  // --- Metadata ---
  let title: string | undefined;
  try {
    const meta = await doc.getMetadata();
    const info = meta.info as Record<string, unknown>;
    if (typeof info?.Title === "string" && info.Title.trim().length > 0) {
      title = info.Title.trim();
    }
  } catch {
    // metadata unavailable — fall back to filename
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
      if (!("str" in item)) continue;
      const textItem = item as { str: string; transform: number[] };
      const y = textItem.transform[5];

      if (lastY !== null && Math.abs(y - lastY) > 1) {
        if (currentLine.trim().length > 0) {
          lines += currentLine.trim() + "\n";
        }
        currentLine = textItem.str;
      } else {
        currentLine += textItem.str;
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
  const outline = (await doc.getOutline()) as PdfOutlineItem[] | null;
  const toc = outline ? await mapOutlineItems(outline, doc) : [];

  await doc.destroy();

  // Remove empty (image-only / no-text) pages and re-index the rest.
  const filteredSections = sections
    .filter((s) => s.content !== EMPTY_PAGE_HTML)
    .map((s, i) => ({ ...s, index: i }));

  // Build a map from original PDF page index → new filtered section index
  // so TOC entries can be remapped correctly.
  const originalToFiltered = new Map<number, number>();
  filteredSections.forEach((s) => {
    // The original index before filtering was stored as the section's id number.
    const originalIndex = parseInt(s.id.replace("page-", ""), 10) - 1;
    originalToFiltered.set(originalIndex, s.index);
  });

  const remappedToc = remapTocIndices(toc, originalToFiltered);

  return {
    metadata: { title },
    navigation: { tableOfContents: remappedToc, pageCount: filteredSections.length },
    sections: filteredSections,
  };
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
  doc: pdfjsLib.PDFDocumentProxy,
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
        const pageIndex = await doc.getPageIndex(dest[0] as { num: number; gen: number });
        sectionIndex = pageIndex; // 0-based, matches section index (page-1 has index 0)
      }
    } catch {
      // destination unresolvable — leave sectionIndex undefined
    }

    const children = item.items?.length
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
