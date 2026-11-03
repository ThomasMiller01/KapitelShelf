import { useCallback, useRef } from "react";

import type { BookContent } from "../../utils/BookContentModels";

interface BookPageProgress {
  absoluteCurrentPage: number;
  absoluteTotalPages: number;
  getAbsolutePage: (section: number, page: number) => number;
  getLocation: (absolutePage: number) => { page: number; section: number };
  progressPercent: number;
  onTotalPagesChange: (total: number, resized: boolean) => void;
}

/**
 * Estimates the reader's absolute position (page X / page Y) across the entire book.
 *
 * EPUBs don't have fixed pages — they're split into sections, and each section is
 * paginated dynamically by the CSS columns layout based on the current viewport.
 * This means we can't know the page count for a section until it is rendered.
 *
 * Strategy:
 *   1. Record "chars per page" from a measured section and update it when the
 *      reading area changes size.
 *   2. Every section's page count is estimated as:
 *        round(sectionChars / charsPerPage)
 *      using only the character length of the section's HTML content as a proxy
 *      for its length — no DOM rendering needed for unvisited sections.
 *   3. absoluteCurrentPage = sum of estimated pages before the current section
 *        + actual page within the current section.
 *   4. Use the measured page count for the current section and estimates for others.
 *
 * Trade-offs:
 *   - The total is an estimate, not a hard truth (pages depend on font size, viewport).
 *   - The estimate can change when the reading layout changes.
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

  const charsPerPageRef = useRef<number | null>(null);

  const sectionChars = content.sections.map((s) => (s.content ?? "").length);

  const onTotalPagesChange = useCallback(
    (total: number, resized: boolean) => {
      const section = currentSectionRef.current;
      if ((charsPerPageRef.current === null || resized) && total > 0 && sectionChars[section] > 0) {
        charsPerPageRef.current = sectionChars[section] / total;
      }
    },
    [sectionChars]
  );

  const charsPerPage =
    charsPerPageRef.current ?? sectionChars[currentSection] / totalPages;

  const estimatedSectionPages = sectionChars.map((chars, index) =>
    index === currentSection ? Math.max(1, totalPages) : Math.max(1, Math.round(chars / charsPerPage) || 1)
  );

  const absoluteCurrentPage =
    estimatedSectionPages.slice(0, currentSection).reduce((sum, p) => sum + p, 0) +
    currentPage +
    1;

  const absoluteTotalPages = estimatedSectionPages.reduce((sum, p) => sum + p, 0);

  const getAbsolutePage = (section: number, page: number): number => {
    const safeSection = Math.min(
      Math.max(section, 0),
      estimatedSectionPages.length - 1,
    );
    const pagesBefore = estimatedSectionPages
      .slice(0, safeSection)
      .reduce((sum, pages) => sum + pages, 0);

    return pagesBefore + Math.min(Math.max(page, 0), estimatedSectionPages[safeSection] - 1) + 1;
  };

  const getLocation = (
    absolutePage: number,
  ): { page: number; section: number } => {
    let remainingPages = Math.min(
      Math.max(Math.round(absolutePage), 1),
      absoluteTotalPages,
    );

    for (let section = 0; section < estimatedSectionPages.length; section += 1) {
      if (remainingPages <= estimatedSectionPages[section]) {
        return { page: remainingPages - 1, section };
      }

      remainingPages -= estimatedSectionPages[section];
    }

    return { page: 0, section: 0 };
  };

  const progressPercent =
    absoluteTotalPages > 0
      ? Math.round((absoluteCurrentPage / absoluteTotalPages) * 100)
      : 0;

  return {
    absoluteCurrentPage,
    absoluteTotalPages,
    getAbsolutePage,
    getLocation,
    progressPercent,
    onTotalPagesChange,
  };
}
