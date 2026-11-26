import { useCallback, useEffect, useReducer, useRef } from "react";
import { useSearchParams } from "react-router-dom";

import type { ReadingBookDTO } from "../../../lib/api/KapitelShelf.Api";
import { changePosition, createPosition, type ReaderLocation, readLocation } from "../utils/readerPosition";

interface ReaderPosition extends ReaderLocation {
  pageCount: number | null;
  canGoBack: boolean;
  canGoForward: boolean;
  jump: (section: number, page: number) => void;
  previous: () => void;
  next: () => void;
  measure: (section: number, pageCount: number) => void;
  lastMove?: ReaderLocation;
}

export const useReaderPosition = (sectionCount: number, saved?: ReadingBookDTO): ReaderPosition => {
  const [params, setParams] = useSearchParams();
  const search = params.toString();
  const knownSearch = useRef(search);
  const savedSection = saved?.currentSection ?? 0;
  const savedPage = saved?.currentPage ?? 0;
  const [position, dispatch] = useReducer(changePosition, undefined, () =>
    createPosition(sectionCount, readLocation(params) ?? { section: savedSection, page: savedPage }),
  );
  const { section, page, pageCount, lastMove } = position;
  const previous = useCallback((): void => dispatch({ type: "turn", direction: -1 }), []);
  const next = useCallback((): void => dispatch({ type: "turn", direction: 1 }), []);
  const measure = useCallback((section: number, pageCount: number): void => dispatch({ type: "measure", section, pageCount }), []);

  useEffect(() => {
    if (search !== knownSearch.current) {
      knownSearch.current = search;
      dispatch({ type: "open", location: readLocation(new URLSearchParams(search)) ?? { section: savedSection, page: savedPage } });
      return;
    }
    if (pageCount === null || page === "last") {
      return;
    }

    const updated = new URLSearchParams(search);
    updated.set("section", String(section));
    updated.set("page", String(page));
    if (updated.toString() !== search) {
      knownSearch.current = updated.toString();
      setParams(updated, { replace: true });
    }
  }, [search, section, page, pageCount, savedSection, savedPage, setParams]);

  useEffect(() => {
    const keyDown = (event: KeyboardEvent): void => {
      if (event.defaultPrevented || event.altKey || event.ctrlKey || event.metaKey || event.shiftKey
        || event.composedPath().some((node) => node instanceof Element && node.matches("a,button,input,select,textarea,[contenteditable],[role=slider]"))) {
        return;
      }
      if (event.key === "ArrowLeft" || event.key === "ArrowRight") {
        event.preventDefault();
        (event.key === "ArrowLeft" ? previous : next)();
      }
    };
    window.addEventListener("keydown", keyDown);
    return (): void => window.removeEventListener("keydown", keyDown);
  }, [previous, next]);

  return {
    section, page: page === "last" ? 0 : page, pageCount, lastMove, previous, next, measure,
    canGoBack: pageCount !== null && (section > 0 || page !== 0),
    canGoForward: pageCount !== null && (section < sectionCount - 1 || page !== pageCount - 1),
    jump: (section, page): void => dispatch({ type: "jump", location: { section, page } }),
  };
};
