import { FileInfoDTO } from "../../lib/api/KapitelShelf.Api";
import { BookContent, BookSection, BookTocItem } from "./BookContentModels";
import { MAX_SECTION_CHARS, SplitBookSections } from "./SectionSplit";

// ALL_CAPS short text (e.g. "THE BEGINNING", "PART ONE").
const ALL_CAPS = /^[A-Z0-9\s.,\-:'"!?]+$/;

// Explicit numbering prefix: "1.", "XIV.", "3)", "II:"
// The [.):\s] suffix prevents false-matching words that use roman-numeral
// letters (e.g. "mix", "civil", "dim").
const NUMBERED_HEADING = /^(\d+|[IVXLCDMivxlcdm]+)[.):\s]/;

// Matches a line that is solely a bare number or roman numeral (used for
// title extraction: skip it and prefer the next heading line).
const BARE_NUMBER = /^(\d+|[IVXLCDMivxlcdm]+)\.?\s*$/i;

export const ParseTxt = async (
  file: File | undefined,
  fileInfo: FileInfoDTO | undefined,
): Promise<BookContent | undefined> => {
  if (file === undefined) {
    return undefined;
  }

  const raw = await file.text();
  const text = raw.replace(/\r\n/g, "\n").replace(/\r/g, "\n");

  const blocks = text
    .split(/\n{2,}/)
    .map((b) => b.trim())
    .filter((b) => b.length > 0);

  const title =
    fileInfo?.fileName?.replace(/\.[^.]+$/, "") ?? "Untitled";

  const sectionGroups = splitIntoSections(blocks);
  const rawSections = buildSections(sectionGroups);
  const splitResult = SplitBookSections(rawSections, MAX_SECTION_CHARS);
  const sections = splitResult.sections;
  const toc = remapTocToSplitSections(
    buildToc(sectionGroups),
    splitResult.sourceToFirstSplitIndex,
  );

  const result: BookContent = {
    metadata: { title },
    navigation: { tableOfContents: toc },
    sections,
  };

  return result;
};

interface SectionGroup {
  title?: string;
  blocks: string[];
}

// Tests a single line against all structural heading signals.
// Language-agnostic: relies on shape (length, punctuation, casing),
// not on language-specific keywords.
// Called for standalone heading blocks and for individual lines within
// a multi-line block via extractBlockHeading.
const isHeadingLine = (line: string): boolean => {
  if (line.length > 80) return false;

  const wordCount = line.split(/\s+/).length;

  // ALL_CAPS short text: a universal typographic convention for titles
  // (e.g. "THE BEGINNING", "PART ONE"). Word count guard avoids matching
  // all-caps sentences or acronym-heavy paragraphs.
  if (ALL_CAPS.test(line) && wordCount <= 8) return true;

  // Explicit numbering prefix: "1.", "XIV.", "3)", "II:"
  if (NUMBERED_HEADING.test(line)) return true;

  // Very few words with no terminal punctuation: isolated 1–4 word blocks
  // are almost always titles ("Winter", "The Storm", "Part Two").
  // Prose sentences this short without a period are extremely rare.
  if (wordCount <= 4 && !/[.!?…]$/.test(line)) return true;

  // Short text with no terminal punctuation: catches longer-but-still-short
  // headings (e.g. "The Long Road to the Sea", 5–8 words).
  // Prose sentences almost always end with . ! ? or …; titles don't.
  if (line.length <= 60 && !/[.!?…]$/.test(line)) return true;

  return false;
};

// Determines whether a paragraph block is a standalone section heading.
// Uses structural signals only — no language-specific keywords — so it
// works for any language (English, German, French, etc.).
//
// All checks operate on blocks that are already separated by 2+ blank lines,
// so we are only ever inspecting text that stands alone on its own.
const isHeading = (block: string): boolean => {
  // Headings are always a single line; multi-line blocks are body text.
  if (block.includes("\n")) return false;
  return isHeadingLine(block);
};

// Given an array of heading lines, returns the title string for the TOC.
// If the first line is a bare number or roman numeral (e.g. "1", "XIV"),
// it is combined with the next line to form "number. title"
// (e.g. ["1", "Eine Kugel aus Feuer"] → "1. Eine Kugel aus Feuer").
const extractTitle = (headingLines: string[]): string => {
  if (headingLines.length > 1 && BARE_NUMBER.test(headingLines[0])) {
    return `${headingLines[0]}. ${headingLines[1]}`;
  }
  return headingLines[0];
};

// Inspects the leading lines of a multi-line block to detect an embedded
// heading prefix. Many TXT books place the chapter number/title on
// single-newline-separated lines immediately before the first body sentence,
// without a blank line separating them — so they end up in the same block
// after the \n{2+} split. This function extracts those heading lines.
//
// Returns { title, bodyBlock } if a heading prefix is found, or null.
// A maximum of 4 consecutive heading lines are consumed (number, title,
// subtitle, location/date are common patterns).
const extractBlockHeading = (
  block: string,
): { title: string; bodyBlock: string } | null => {
  if (!block.includes("\n")) return null;

  const lines = block
    .split("\n")
    .map((l) => l.trim())
    .filter((l) => l.length > 0);

  if (lines.length < 2) return null;

  let headingCount = 0;
  for (const line of lines) {
    if (headingCount < 4 && isHeadingLine(line)) {
      headingCount++;
    } else {
      break;
    }
  }

  // Need at least 1 heading line and at least 1 body line remaining.
  if (headingCount === 0 || headingCount === lines.length) return null;

  return {
    title: extractTitle(lines.slice(0, headingCount)),
    bodyBlock: lines.slice(headingCount).join("\n"),
  };
};

// Splits a section group into chunks of at most MAX_SECTION_CHARS.
// Only the first chunk inherits the heading title, so the TOC entry
// still points to the start of the chapter even when it is sub-split.
const chunkGroup = (group: SectionGroup): SectionGroup[] => {
  const result: SectionGroup[] = [];
  let current: string[] = [];
  let currentChars = 0;
  let isFirst = true;

  const flush = () => {
    if (current.length === 0) return;
    result.push({ title: isFirst ? group.title : undefined, blocks: current });
    current = [];
    currentChars = 0;
    isFirst = false;
  };

  for (const block of group.blocks) {
    if (currentChars + block.length > MAX_SECTION_CHARS && current.length > 0) {
      flush();
    }
    current.push(block);
    currentChars += block.length;
  }
  flush();

  return result.length > 0 ? result : [{ title: group.title, blocks: [] }];
};

// Two paths:
//   - Headings found (standalone block or embedded prefix): each heading
//     starts a new SectionGroup; content before the first heading becomes
//     an untitled preamble.
//   - No headings: the entire book is one group.
// In both cases every group is passed through chunkGroup to enforce the
// section size cap, so even headingless books are properly split.
const splitIntoSections = (blocks: string[]): SectionGroup[] => {
  const groups: SectionGroup[] = [];
  let current: SectionGroup | null = null;

  const ensureGroup = () => {
    if (current === null) {
      current = { blocks: [] };
      groups.push(current);
    }
  };

  for (const block of blocks) {
    if (isHeading(block)) {
      // Entire block is a standalone heading (single line, no body).
      current = { title: block, blocks: [] };
      groups.push(current);
    } else {
      const embedded = extractBlockHeading(block);
      if (embedded !== null) {
        // Block begins with heading lines followed by body text.
        current = { title: embedded.title, blocks: [block] };
        groups.push(current);
      } else {
        // Plain body block — append to current group.
        ensureGroup();
        current!.blocks.push(block);
      }
    }
  }

  return (groups.length > 0 ? groups : [{ blocks }]).flatMap(chunkGroup);
};

const escapeHtml = (text: string): string =>
  text
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");

const blocksToHtml = (blocks: string[]): string =>
  blocks
    .map((block) => `<p>${escapeHtml(block).replace(/\n/g, "<br>")}</p>`)
    .join("\n");

const buildSections = (groups: SectionGroup[]): BookSection[] =>
  groups.map((group, index) => ({
    id: `section-${index}`,
    index,
    title: group.title,
    contentType: "html",
    content: blocksToHtml(group.blocks),
    sizeBytes: new TextEncoder().encode(group.blocks.join("\n\n")).byteLength,
  }));

const buildToc = (groups: SectionGroup[]): BookTocItem[] =>
  groups
    .filter((g) => g.title !== undefined)
    .map((group, i) => {
      const sectionIndex = groups.indexOf(group);
      return {
        id: `toc-${i}`,
        label: group.title!,
        sectionIndex,
        children: [],
      };
    });

const remapTocToSplitSections = (
  toc: BookTocItem[],
  sourceToFirstSplitIndex: Map<number, number>,
): BookTocItem[] =>
  toc.map((item) => ({
    ...item,
    sectionIndex:
      item.sectionIndex !== undefined
        ? sourceToFirstSplitIndex.get(item.sectionIndex)
        : undefined,
    children: remapTocToSplitSections(item.children, sourceToFirstSplitIndex),
  }));
