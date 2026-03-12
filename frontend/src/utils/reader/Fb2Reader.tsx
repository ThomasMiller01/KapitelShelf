import type { Fb2Book, Fb2Section, Fb2TocItem } from "foliate-js/fb2.js";
import { makeFB2 } from "foliate-js/fb2.js";
import type { FileInfoDTO } from "../../lib/api/KapitelShelf.Api";
import type { BookContent, BookSection, BookTocItem } from "./BookContentModels";
import { MAX_SECTION_CHARS, SplitBookSections } from "./SectionSplit";

export const ParseFb2 = async (
  file: File | undefined,
  _fileInfo: FileInfoDTO | undefined,
): Promise<BookContent | undefined> => {
  if (file === undefined) {
    return undefined;
  }

  const book: Fb2Book = await makeFB2(file);

  if (!Array.isArray(book.sections)) {
    return undefined;
  }

  const rawSections = await mapSections(book.sections);
  const splitResult = SplitBookSections(rawSections, MAX_SECTION_CHARS);
  const sections = splitResult.sections;

  const result: BookContent = {
    metadata: {
      title: book.metadata?.title ?? undefined,
      description: book.metadata?.description ?? undefined,
    },
    navigation: {
      tableOfContents: mapTocItems(
        book.toc,
        { value: 0 },
        splitResult.sourceToFirstSplitIndex,
      ),
      pageCount: undefined,
    },
    sections,
  };

  book.destroy();
  return result;
};

const mapTocItems = (
  toc: Fb2TocItem[] | undefined | null,
  counter: { value: number },
  sourceToFirstSplitIndex: Map<number, number>,
): BookTocItem[] => {
  if (!toc) {
    return [];
  }
  return toc.map((item): BookTocItem => {
    // FB2 TOC href is the section index as a string, e.g. "3" or "3#1".
    // Parse the part before "#" to get the section index.
    const parsed =
      item.href !== undefined ? parseInt(item.href.split("#")[0], 10) : undefined;
    const sourceSectionIndex =
      parsed !== undefined && !Number.isNaN(parsed) ? parsed : undefined;
    const sectionIndex =
      sourceSectionIndex !== undefined
        ? sourceToFirstSplitIndex.get(sourceSectionIndex)
        : undefined;

    return {
      id: `toc-${counter.value++}`,
      label: item.label ?? "Untitled",
      href: item.href,
      sectionIndex,
      children: mapTocItems(item.subitems, counter, sourceToFirstSplitIndex),
    };
  });
};

const mapSections = async (sections: Fb2Section[]): Promise<BookSection[]> => {
  const result: BookSection[] = [];

  for (let index = 0; index < sections.length; index++) {
    const section = sections[index];
    // load() is synchronous for FB2 — returns a blob URL directly.
    const blobUrl = section.load();
    const response = await fetch(blobUrl);
    const content = await response.text();
    result.push({
      id: String(section.id),
      index,
      title: undefined,
      href: String(section.id),
      contentType: "xhtml",
      content,
      text: undefined,
      sizeBytes: section.size,
    });
  }

  return result;
};
