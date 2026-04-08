import type { BookSection } from "./BookContentModels";

export const MAX_SECTION_CHARS = 50_000;

const PARAGRAPH_WRAPPER_CHARS = "<p></p>".length;

export interface SplitSectionsResult {
  sections: BookSection[];
  sourceToFirstSplitIndex: Map<number, number>;
}

export const SplitBookSections = (
  sections: BookSection[],
  maxChars = MAX_SECTION_CHARS,
): SplitSectionsResult => {
  const encoder = new TextEncoder();
  const result: BookSection[] = [];
  const sourceToFirstSplitIndex = new Map<number, number>();

  for (const section of sections) {
    const sourceSectionIndex = section.index;
    const chunks = enforceHardLimit(
      splitSectionContent(section, maxChars),
      maxChars,
    );
    const isSplit = chunks.length > 1;

    for (let chunkIndex = 0; chunkIndex < chunks.length; chunkIndex++) {
      const chunk = chunks[chunkIndex];
      const isFirstChunk = chunkIndex === 0;
      const nextIndex = result.length;

      if (isFirstChunk) {
        sourceToFirstSplitIndex.set(sourceSectionIndex, nextIndex);
      }

      result.push({
        ...section,
        id: isFirstChunk ? section.id : `${section.id}__chunk-${chunkIndex}`,
        index: nextIndex,
        title: isFirstChunk ? section.title : undefined,
        href: isFirstChunk ? section.href : undefined,
        locator: isFirstChunk ? section.locator : undefined,
        content: chunk,
        text: isSplit ? undefined : section.text,
        sizeBytes: encoder.encode(chunk).byteLength,
        sourceSectionIndex,
      });
    }
  }

  return {
    sections: result,
    sourceToFirstSplitIndex,
  };
};

const splitSectionContent = (
  section: BookSection,
  maxChars: number,
): string[] => {
  if (section.content.length <= maxChars) {
    return [section.content];
  }

  switch (section.contentType) {
    case "xhtml":
    case "html":
      return splitMarkupContent(section.content, maxChars);
    case "pdf-page":
    case "text":
      return splitTextualContent(section.content, maxChars);
    default:
      return splitStringByMax(section.content, maxChars);
  }
};

const splitMarkupContent = (content: string, maxChars: number): string[] => {
  const bodyMatch = content.match(
    /^([\s\S]*?<body\b[^>]*>)([\s\S]*?)(<\/body>[\s\S]*)$/i,
  );
  const prefix = bodyMatch?.[1] ?? "";
  const body = bodyMatch?.[2] ?? content;
  const suffix = bodyMatch?.[3] ?? "";

  const maxBodyChars = maxChars - prefix.length - suffix.length;
  if (maxBodyChars <= 0) {
    return splitStringByMax(content, maxChars);
  }

  const units = getMarkupUnits(body);
  if (units.length === 0) {
    return splitStringByMax(content, maxChars);
  }

  const bodyChunks = packUnits(units, maxBodyChars, splitMarkupUnitHard);
  if (bodyChunks.length === 0) {
    return splitStringByMax(content, maxChars);
  }

  return bodyChunks.map((chunkBody) => `${prefix}${chunkBody}${suffix}`);
};

const splitTextualContent = (content: string, maxChars: number): string[] => {
  const paragraphUnits = extractParagraphUnits(content);
  if (paragraphUnits.length > 0) {
    return packUnits(paragraphUnits, maxChars, splitParagraphUnitHard);
  }

  const plainText = stripHtml(content).trim();
  if (plainText.length === 0) {
    return splitStringByMax(content, maxChars);
  }

  const lines = plainText
    .split(/\n+/)
    .map((line) => line.trim())
    .filter((line) => line.length > 0);

  if (lines.length > 1) {
    const lineUnits = lines.map((line) => `<p>${escapeHtml(line)}</p>`);
    return packUnits(lineUnits, maxChars, splitParagraphUnitHard);
  }

  const plainChunks = splitStringByMax(
    plainText,
    Math.max(1, maxChars - PARAGRAPH_WRAPPER_CHARS),
  );
  return plainChunks.map((chunk) => `<p>${escapeHtml(chunk)}</p>`);
};

const packUnits = (
  units: string[],
  maxChars: number,
  splitOversizedUnit: (unit: string, maxChars: number) => string[],
): string[] => {
  const chunks: string[] = [];
  let currentChunk = "";

  const flush = () => {
    if (currentChunk.length > 0) {
      chunks.push(currentChunk);
      currentChunk = "";
    }
  };

  for (const unit of units) {
    if (unit.length > maxChars) {
      flush();
      const oversizedChunks = splitOversizedUnit(unit, maxChars).filter(
        (chunk) => chunk.length > 0,
      );
      chunks.push(...oversizedChunks);
      continue;
    }

    if (currentChunk.length > 0 && currentChunk.length + unit.length > maxChars) {
      flush();
    }
    currentChunk += unit;
  }

  flush();
  return chunks;
};

const splitMarkupUnitHard = (unit: string, maxChars: number): string[] => {
  const plainText = stripHtml(unit).trim();
  if (plainText.length === 0) {
    return splitStringByMax(unit, maxChars);
  }

  const textChunks = splitStringByMax(
    plainText,
    Math.max(1, maxChars - PARAGRAPH_WRAPPER_CHARS),
  );
  return textChunks.map((chunk) => `<p>${escapeHtml(chunk)}</p>`);
};

const splitParagraphUnitHard = (unit: string, maxChars: number): string[] => {
  const plainText = stripHtml(unit).trim();
  if (plainText.length === 0) {
    return splitStringByMax(unit, maxChars);
  }

  const textChunks = splitStringByMax(
    plainText,
    Math.max(1, maxChars - PARAGRAPH_WRAPPER_CHARS),
  );
  return textChunks.map((chunk) => `<p>${escapeHtml(chunk)}</p>`);
};

const getMarkupUnits = (innerMarkup: string): string[] => {
  const parser = new DOMParser();
  const doc = parser.parseFromString(`<body>${innerMarkup}</body>`, "text/html");
  const units = Array.from(doc.body.childNodes)
    .map(serializeNode)
    .map((chunk) => chunk.trim())
    .filter((chunk) => chunk.length > 0);

  if (units.length > 0) {
    return units;
  }

  const fallback = innerMarkup.trim();
  return fallback.length > 0 ? [fallback] : [];
};

const serializeNode = (node: ChildNode): string => {
  if ("outerHTML" in node && typeof node.outerHTML === "string") {
    return node.outerHTML;
  }
  return escapeHtml(node.textContent ?? "");
};

const extractParagraphUnits = (content: string): string[] => {
  const matches = content.match(/<p\b[^>]*>[\s\S]*?<\/p>/gi);
  if (!matches) {
    return [];
  }
  return matches.map((paragraph) => paragraph.trim()).filter((p) => p.length > 0);
};

const enforceHardLimit = (chunks: string[], maxChars: number): string[] => {
  const result: string[] = [];

  for (const chunk of chunks) {
    if (chunk.length <= maxChars) {
      result.push(chunk);
    } else {
      result.push(...splitStringByMax(chunk, maxChars));
    }
  }

  return result.filter((chunk) => chunk.length > 0);
};

const splitStringByMax = (value: string, maxChars: number): string[] => {
  if (value.length <= maxChars) {
    return [value];
  }

  const result: string[] = [];
  for (let index = 0; index < value.length; index += maxChars) {
    result.push(value.slice(index, index + maxChars));
  }
  return result;
};

const stripHtml = (value: string): string => {
  const parser = new DOMParser();
  const doc = parser.parseFromString(value, "text/html");
  return doc.body.textContent ?? "";
};

const escapeHtml = (text: string): string =>
  text
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");
