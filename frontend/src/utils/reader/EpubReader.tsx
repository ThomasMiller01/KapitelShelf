import { EPUB } from "foliate-js/epub.js";
import JSZip from "jszip";
import { FileInfoDTO } from "../../lib/api/KapitelShelf.Api";
import { BookContent, BookSection, BookTocItem } from "./BookContentModels";

interface FoliateTocItem {
  label?: string;
  href?: string;
  subitems?: FoliateTocItem[];
}

interface FoliateSection {
  id: string;
  load: () => Promise<string>;
  size: number;
  cfi?: string;
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
  file: File | undefined,
  fileInfo: FileInfoDTO | undefined,
): Promise<BookContent | undefined> => {
  if (file === undefined) {
    return undefined;
  }

  const zip = await JSZip.loadAsync(await file.arrayBuffer());

  const loadText = async (path: string): Promise<string> => {
    const entry = zip.file(path);
    if (entry === null) {
      return "";
    }

    return await entry.async("string");
  };

  const loadBlob = async (path: string): Promise<Blob> => {
    const entry = zip.file(path);
    if (entry === null) {
      throw new Error(`EPUB resource not found: ${path}`);
    }

    const data = await entry.async("uint8array");
    const arrayBuffer = data.buffer.slice(
      data.byteOffset,
      data.byteOffset + data.byteLength,
    ) as ArrayBuffer;
    return new Blob([arrayBuffer], {
      type: fileInfo?.mimeType ?? "application/octet-stream",
    });
  };

  const epub = new EPUB({
    loadText,
    loadBlob,
    getSize: () => fileInfo?.fileSizeBytes ?? 0,
  });

  const parsed = (await epub.init()) as FoliateParsedBook;
  if (!Array.isArray(parsed.sections)) {
    return undefined;
  }

  const sections = await mapSections(parsed.sections);
  const sectionIndexMap = new Map<string, number>();
  for (const section of sections) {
    if (section.href) {
      sectionIndexMap.set(section.href.split("#")[0], section.index);
    }
  }

  const result: BookContent = {
    metadata: {
      title: parsed.metadata?.title,
      subtitle: parsed.metadata?.subtitle,
      description: parsed.metadata?.description,
    },
    navigation: {
      tableOfContents: mapTocItems(parsed.toc, sectionIndexMap),
      pageCount: undefined,
    },
    sections,
  };

  return result;
};

const mapTocItems = (
  toc: FoliateTocItem[] | undefined,
  sectionIndexMap: Map<string, number>,
): BookTocItem[] => {
  return (toc ?? []).map((item: FoliateTocItem, index: number): BookTocItem => {
    const hrefWithoutHash = item.href?.split("#")[0];
    const sectionIndex =
      hrefWithoutHash !== undefined
        ? sectionIndexMap.get(hrefWithoutHash)
        : undefined;

    return {
      id: `${index}-${item.href ?? item.label ?? "toc"}`,
      label: item.label ?? "Untitled",
      href: item.href,
      sectionIndex,
      children: mapTocItems(item.subitems, sectionIndexMap),
    };
  });
};

const mapSections = async (
  sections: FoliateSection[],
): Promise<BookSection[]> => {
  const result: BookSection[] = [];

  for (let index = 0; index < sections.length; index++) {
    const section = sections[index];
    const blobUrl = await section.load();
    const response = await fetch(blobUrl);
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
