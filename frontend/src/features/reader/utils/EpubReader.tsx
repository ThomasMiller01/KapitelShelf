import { EPUB, type FoliateSection } from "foliate-js/epub.js";
import JSZip from "jszip";

import type { BookContent, BookSection, BookTocItem } from "./BookContentModels";
import { MAX_SECTION_CHARS, SplitBookSections } from "./SectionSplit";

interface FoliateTocItem {
  label?: string;
  href?: string;
  subitems?: FoliateTocItem[];
}

interface FoliateParsedBook {
  metadata?: {
    title?: string;
    subtitle?: string;
    description?: string;
    language?: string | string[];
    identifier?: string;
    author?: { name?: string }[] | { name?: string } | string[] | string;
    publisher?: { name?: string }[] | { name?: string } | string[] | string;
    subject?: { name?: string }[] | { name?: string } | string[] | string;
    published?: string;
  };
  toc?: FoliateTocItem[];
  sections: FoliateSection[];
}

export const ParseEpub = async (
  file: File,
  signal: AbortSignal,
): Promise<BookContent> => {
  const bytes = await file.arrayBuffer();
  signal.throwIfAborted();
  const zip = await JSZip.loadAsync(bytes);

  const loadText = async (path: string): Promise<string> => {
    signal.throwIfAborted();
    const entry = zip.file(path);
    if (entry === null) {
      return "";
    }

    return entry.async("string");
  };

  const loadBlob = async (path: string): Promise<Blob> => {
    signal.throwIfAborted();
    const entry = zip.file(path);
    if (entry === null) {
      throw new Error(`EPUB resource not found: ${path}`);
    }

    const data = await entry.async("uint8array");
    const arrayBuffer = data.buffer.slice(
      data.byteOffset,
      data.byteOffset + data.byteLength,
    ) as ArrayBuffer;
    return new Blob([arrayBuffer]);
  };

  const epub = new EPUB({
    loadText,
    loadBlob,
    getSize: (): number => file.size,
  });

  try {
    const parsed = (await epub.init()) as FoliateParsedBook;
    signal.throwIfAborted();
    const rawSections = await mapSections(parsed.sections, signal);
    const tocSectionIndexMap = new Map<string, number>();
    for (const section of rawSections) {
      if (section.href) {
        tocSectionIndexMap.set(section.href.split("#")[0], section.index);
      }
    }
    const { sections, sourceToFirstSplitIndex } = SplitBookSections(rawSections, MAX_SECTION_CHARS);

    return {
      metadata: {
        title: parsed.metadata?.title,
        subtitle: parsed.metadata?.subtitle,
        description: parsed.metadata?.description,
      },
      navigation: {
        tableOfContents: mapTocItems(parsed.toc, tocSectionIndexMap, sourceToFirstSplitIndex),
      },
      sections,
      // Packaged image URLs stay valid until the reader releases this book.
      release: (): void => epub.destroy(),
    };
  } catch (error) {
    epub.destroy();
    throw error;
  }
};

const mapTocItems = (
  toc: FoliateTocItem[] | undefined,
  sectionIndexMap: Map<string, number>,
  sourceToFirstSplitIndex: Map<number, number>,
): BookTocItem[] =>
  (toc ?? []).map((item: FoliateTocItem, index: number): BookTocItem => {
    const hrefWithoutHash = item.href?.split("#")[0];
    const sourceSectionIndex =
      hrefWithoutHash !== undefined
        ? sectionIndexMap.get(hrefWithoutHash)
        : undefined;
    const sectionIndex =
      sourceSectionIndex !== undefined
        ? sourceToFirstSplitIndex.get(sourceSectionIndex)
        : undefined;

    return {
      id: `${index}-${item.href ?? item.label ?? "toc"}`,
      label: item.label ?? "Untitled",
      href: item.href,
      sectionIndex,
      children: mapTocItems(item.subitems, sectionIndexMap, sourceToFirstSplitIndex),
    };
  });

const mapSections = async (
  sections: FoliateSection[],
  signal: AbortSignal,
): Promise<BookSection[]> => {
  const result: BookSection[] = [];

  for (let index = 0; index < sections.length; index++) {
    signal.throwIfAborted();
    const section = sections[index];
    const blobUrl = await section.load();
    const response = await fetch(blobUrl, { signal });
    const content = await response.text();
    result.push({
      id: section.id,
      index,
      title: undefined,
      href: section.id,
      contentType: "xhtml",
      content,
      text: undefined,
      sizeBytes: section.size,
      locator: {
        locationHint: section.cfi,
        progression: undefined,
      },
    });
  }

  return result;
};
