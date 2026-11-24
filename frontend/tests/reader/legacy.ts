import type { BookContent, BookTocItem } from "../../src/features/reader/utils/BookContentModels";
import { ParseEpub } from "../../src/features/reader/utils/EpubReader";
import { ParseFb2 } from "../../src/features/reader/utils/Fb2Reader";
import { ParsePdf } from "../../src/features/reader/utils/PdfReader";
import { ParseTxt } from "../../src/features/reader/utils/TxtReader";
import { epubFixture, fb2Fixture, pdfFixture, txtFixture } from "./fixtures";

const results = document.querySelector<HTMLPreElement>("#results")!;
const fixtures = document.querySelector<HTMLUListElement>("#fixtures")!;

const tocLocations = (items: BookTocItem[]): (number | null)[] =>
  items.flatMap((item) => [item.sectionIndex ?? null, ...tocLocations(item.children)]);

const check = (actual: unknown, expected: unknown): void => {
  if (JSON.stringify(actual) !== JSON.stringify(expected)) {
    throw new Error(`Expected ${JSON.stringify(expected)}, got ${JSON.stringify(actual)}`);
  }
};

const cases = [
  {
    file: epubFixture(),
    parse: ParseEpub,
    sections: [[0, "first.xhtml", 0], [1, "first.xhtml__chunk-1", 0], [2, "second.xhtml", 1]],
    toc: [0, 0, 2],
    content: (book: BookContent): boolean => book.sections[2].content.includes('alt="Packaged illustration"'),
  },
  {
    file: fb2Fixture(),
    parse: ParseFb2,
    sections: [[0, "0", 0], [1, "1", 1], [2, "2", 2]],
    toc: [0, 0, 1, 2],
    content: (book: BookContent): boolean => book.sections[2].content.includes("One note."),
  },
  {
    file: txtFixture(),
    parse: ParseTxt,
    sections: [[0, "section-0", 0], [1, "section-1", 1], [2, "section-2", 2], [3, "section-3", 3]],
    toc: [1, 3],
    content: (book: BookContent): boolean => book.sections[3].content.includes("café and 雪"),
  },
  {
    file: pdfFixture(),
    parse: ParsePdf,
    sections: [[0, "page-1", 0], [1, "page-3", 1]],
    toc: [null, 1],
    content: (book: BookContent): boolean => book.sections[1].text === "Third PDF page",
  },
  {
    file: pdfFixture(false),
    parse: ParsePdf,
    sections: [[0, "pdf-no-text", null]],
    toc: [],
    content: (book: BookContent): boolean => book.sections[0].content.includes("No extractable text"),
  },
];

const run = async (): Promise<void> => {
  const lines: string[] = [];
  let failed = false;
  for (const entry of cases) {
    const file = await entry.file;
    const link = document.createElement("a");
    link.href = URL.createObjectURL(file);
    link.download = file.name;
    link.textContent = file.name;
    const item = document.createElement("li");
    item.append(link);
    fixtures.append(item);

    try {
      const book = await entry.parse(file, { fileName: file.name, mimeType: file.type, fileSizeBytes: file.size });
      if (!book) {
        throw new Error("Parser returned no book");
      }
      check(book.sections.map((section) => [section.index, section.id, section.sourceSectionIndex ?? null]), entry.sections);
      check(tocLocations(book.navigation.tableOfContents), entry.toc);
      check(entry.content(book), true);
      lines.push(`PASS ${file.name}`);
    } catch (error) {
      failed = true;
      lines.push(`FAIL ${file.name}: ${error instanceof Error ? error.message : String(error)}`);
    }
    results.textContent = lines.join("\n");
  }
  results.dataset.status = failed ? "failed" : "passed";
};

void run().catch((error: unknown) => {
  results.textContent = String(error);
  results.dataset.status = "failed";
});
