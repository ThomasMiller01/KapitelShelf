import { useCallback, useEffect, useLayoutEffect, useRef } from "react";

interface UseReaderNavigationArgs {
  currentSection: number;
  currentPage: number;
  totalPages: number;
  setCurrentPage: (page: number) => void;
  nextSection: () => void;
  prevSection: () => void;
  setTotalPages: (total: number) => void;
  onPageProgressChange: (total: number, resized: boolean) => void;
}

interface ReaderNavigationState {
  handleNext: () => void;
  handlePrev: () => void;
  handleTotalPagesChange: (total: number, resized: boolean) => void;
}

export const useReaderNavigation = ({
  currentSection,
  currentPage,
  totalPages,
  setCurrentPage,
  nextSection,
  prevSection,
  setTotalPages,
  onPageProgressChange,
}: UseReaderNavigationArgs): ReaderNavigationState => {
  const navigatedBackRef = useRef(false);
  const isInitialMountRef = useRef(true);

  useEffect(() => {
    if (isInitialMountRef.current) {
      isInitialMountRef.current = false;
      return; // Preserve page from URL on initial load
    }

    if (navigatedBackRef.current) {
      navigatedBackRef.current = false;
      return; // handleTotalPagesChange already set the correct last page
    }
  }, [currentSection]);

  const handleTotalPagesChange = useCallback(
    (total: number, resized: boolean) => {
      setTotalPages(total);
      onPageProgressChange(total, resized);

      if (navigatedBackRef.current) {
        // Don't clear the flag here, let useEffect do it after layout effects settle
        setCurrentPage(total - 1);
      } else if (resized && totalPages > 0 && total !== totalPages) {
        // shortcut: estimates position by section progress, use text anchors for exact restoration.
        setCurrentPage(Math.min(total - 1, Math.floor(currentPage * total / totalPages)));
      } else if (currentPage > total - 1) {
        setCurrentPage(total - 1);
      }
    },
    [currentPage, onPageProgressChange, setCurrentPage, setTotalPages, totalPages],
  );

  const handleNext = (): void => {
    if (currentPage < totalPages - 1) {
      setCurrentPage(currentPage + 1);
    } else {
      nextSection();
    }
  };

  const handlePrev = (): void => {
    if (currentPage > 0) {
      setCurrentPage(currentPage - 1);
    } else {
      navigatedBackRef.current = true;
      prevSection();
    }
  };

  // Keep refs to latest handlers so the keyboard listener is stable (subscribed once).
  const handleNextRef = useRef(handleNext);
  const handlePrevRef = useRef(handlePrev);
  useLayoutEffect(() => { handleNextRef.current = handleNext; });
  useLayoutEffect(() => { handlePrevRef.current = handlePrev; });

  useEffect(() => {
    const onKeyDown = (e: KeyboardEvent): void => {
      if (e.key === "ArrowRight") {
        handleNextRef.current();
      } else if (e.key === "ArrowLeft") {
        handlePrevRef.current();
      }
    };
    window.addEventListener("keydown", onKeyDown);
    return (): void => window.removeEventListener("keydown", onKeyDown);
  }, []);

  return { handleNext, handlePrev, handleTotalPagesChange };
};
