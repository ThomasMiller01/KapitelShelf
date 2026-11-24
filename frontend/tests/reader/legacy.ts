import type { BookContent, BookTocItem } from "../../src/features/reader/utils/BookContentModels";
import { parseBookFile } from "../../src/features/reader/utils/BookParser";
import { checkBookDownload } from "./download";
import { epubFixture, fb2Fixture, pdfFixture, txtFixture } from "./fixtures";
import { checkReaderLoading } from "./loading";

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
    sections: [[0, "first.xhtml", 0], [1, "first.xhtml__chunk-1", 0], [2, "second.xhtml", 1]],
    toc: [0, 0, 2],
    content: (book: BookContent): boolean => book.sections[2].content.includes('alt="Packaged illustration"'),
  },
  {
    file: fb2Fixture(),
    sections: [[0, "0", 0], [1, "1", 1], [2, "2", 2]],
    toc: [0, 0, 1, 2],
    content: (book: BookContent): boolean => book.sections[2].content.includes("One note."),
  },
  {
    file: txtFixture(),
    sections: [[0, "section-0", 0], [1, "section-1", 1], [2, "section-2", 2], [3, "section-3", 3]],
    toc: [1, 3],
    content: (book: BookContent): boolean => book.sections[3].content.includes("café and 雪"),
  },
  {
    file: pdfFixture(),
    sections: [[0, "page-1", 0], [1, "page-3", 1]],
    toc: [null, 1],
    content: (book: BookContent): boolean => book.sections[1].text === "Third PDF page",
  },
  {
    file: pdfFixture(false),
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

    let book: BookContent | undefined;
    try {
      book = await parseBookFile(file, new AbortController().signal);
      check(book.sections.map((section) => [section.index, section.id, section.sourceSectionIndex ?? null]), entry.sections);
      check(tocLocations(book.navigation.tableOfContents), entry.toc);
      check(entry.content(book), true);
      if (book.release) {
        const chapter = document.createElement("template");
        chapter.innerHTML = book.sections[2].content;
        const src = chapter.content.querySelector("img")?.getAttribute("src");
        if (!src?.startsWith("blob:")) {
          throw new Error("The packaged EPUB image is missing");
        }
        check((await fetch(src)).ok, true);
        book.release();
        book.release = undefined;
        check(await fetch(src).then(() => false, () => true), true);
      }
      lines.push(`PASS ${file.name}`);
    } catch (error) {
      failed = true;
      lines.push(`FAIL ${file.name}: ${error instanceof Error ? error.message : String(error)}`);
    } finally {
      book?.release?.();
    }
    results.textContent = lines.join("\n");
  }
  for (const [name, verify] of [
    ["checksum retry, fallback, and cancellation", checkBookDownload],
    ["replaced reader load and single parse", checkReaderLoading],
  ] as const) {
    try {
      await verify();
      lines.push(`PASS ${name}`);
    } catch (error) {
      failed = true;
      lines.push(`FAIL ${name}: ${error instanceof Error ? error.message : String(error)}`);
    }
  }
  results.textContent = lines.join("\n");
  results.dataset.status = failed ? "failed" : "passed";
};

void run().catch((error: unknown) => {
  results.textContent = String(error);
  results.dataset.status = "failed";
});
