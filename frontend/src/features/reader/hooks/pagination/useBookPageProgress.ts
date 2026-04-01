import { useCallback, useRef } from "react";
import { BookContent } from "../../../../utils/reader/BookContentModels";

interface BookPageProgress {
  absoluteCurrentPage: number;
  absoluteTotalPages: number;
  progressPercent: number;
  onTotalPagesChange: (total: number) => void;
}

/**
 * Estimates the reader's absolute position (page X / page Y) across the entire book.
 *
 * EPUBs don't have fixed pages — they're split into sections, and each section is
 * paginated dynamically by the CSS columns layout based on the current viewport.
 * This means we can't know the page count for a section until it is rendered.
 *
 * Strategy:
 *   1. On the first section render, record "chars per page" from the actual measured
 *      page count. This value is frozen for the rest of the session so the total
 *      never changes as more sections are visited.
 *   2. Every section's page count is estimated as:
 *        round(sectionChars / charsPerPage)
 *      using only the character length of the section's HTML content as a proxy
 *      for its length — no DOM rendering needed for unvisited sections.
 *   3. absoluteCurrentPage = sum of estimated pages before the current section
 *        + actual page within the current section.
 *   4. absoluteTotalPages = sum of all estimated section pages.
 *
 * Trade-offs:
 *   - The total is an estimate, not a hard truth (pages depend on font size, viewport).
 *   - It stabilises immediately after the first section renders and never shifts again.
 *   - Jumping to any chapter works correctly since we only need chars, not a render.
 */
export function useBookPageProgress(
  content: BookContent,
  currentSection: number,
  currentPage: number,
  totalPages: number
): BookPageProgress {
  const currentSectionRef = useRef(currentSection);
  currentSectionRef.current = currentSection;

  // Frozen after the first measured section — never updated again.
  const charsPerPageRef = useRef<number | null>(null);

  const sectionChars = content.sections.map((s) => (s.content ?? "").length);

  const onTotalPagesChange = useCallback(
    (total: number) => {
      const section = currentSectionRef.current;
      if (charsPerPageRef.current === null && total > 0 && sectionChars[section] > 0) {
        charsPerPageRef.current = sectionChars[section] / total;
      }
    },
    [sectionChars]
  );

  const charsPerPage =
    charsPerPageRef.current ?? sectionChars[currentSection] / totalPages;

  const estimatedSectionPages = sectionChars.map((chars) =>
    Math.max(1, Math.round(chars / charsPerPage))
  );

  const absoluteCurrentPage =
    estimatedSectionPages.slice(0, currentSection).reduce((sum, p) => sum + p, 0) +
    currentPage +
    1;

  const absoluteTotalPages = estimatedSectionPages.reduce((sum, p) => sum + p, 0);

  const progressPercent =
    absoluteTotalPages > 0
      ? Math.round((absoluteCurrentPage / absoluteTotalPages) * 100)
      : 0;

  return { absoluteCurrentPage, absoluteTotalPages, progressPercent, onTotalPagesChange };
}
