import type React from "react";
import { useLayoutEffect, useRef, useState } from "react";

import type { BookSection } from "../../../utils/bookReader/BookContent";
import type { BoundarySwipeTransition } from "./useSwipeNavigation";

type TransitionDirection = "forward" | "backward";

export interface OutgoingSectionSnapshot {
  section: BookSection;
  page: number;
  direction: TransitionDirection;
}

const SECTION_TRANSITION_MS = 180;

interface UseSectionTransitionArgs {
  section: BookSection;
  sectionIndex: number;
  currentPage: number;
  pageWidthRef: React.RefObject<number>;
  pageStride: number;
  contentRef: React.RefObject<HTMLDivElement | null>;
  boundarySwipeTransitionRef: React.MutableRefObject<BoundarySwipeTransition | null>;
}

interface SectionTransitionState {
  outgoingSnapshot: OutgoingSectionSnapshot | null;
  trackOffset: number;
  animateTrack: boolean;
  isSectionTransitioning: boolean;
  forcedPage: number | null;
}

export const useSectionTransition = ({
  section,
  sectionIndex,
  currentPage,
  pageWidthRef,
  pageStride,
  contentRef,
  boundarySwipeTransitionRef,
}: UseSectionTransitionArgs): SectionTransitionState => {
  const prevSectionRef = useRef(section);
  const prevSectionIndexRef = useRef(sectionIndex);
  const prevPageRef = useRef(currentPage);

  const forcedPageRef = useRef<number | null>(null);
  const transitionTimeoutRef = useRef<number | null>(null);

  const [outgoingSnapshot, setOutgoingSnapshot] =
    useState<OutgoingSectionSnapshot | null>(null);
  const [trackOffset, setTrackOffset] = useState(0);
  const [animateTrack, setAnimateTrack] = useState(false);

  // Once currentPage prop matches the forced value, the URL has caught up — clear the override.
  useLayoutEffect(() => {
    if (
      forcedPageRef.current !== null &&
      currentPage === forcedPageRef.current
    ) {
      forcedPageRef.current = null;
    }
  }, [currentPage]);

  // Keep outgoing section mounted while sliding between sections.
  useLayoutEffect(() => {
    if (
      section === prevSectionRef.current ||
      sectionIndex === prevSectionIndexRef.current ||
      pageWidthRef.current === 0 ||
      pageStride === 0
    ) {
      return;
    }

    const direction: TransitionDirection =
      sectionIndex > prevSectionIndexRef.current ? "forward" : "backward";
    const width = pageWidthRef.current;
    const pageGap = Math.max(0, pageStride - width);
    const targetTrackOffset = direction === "forward" ? -pageStride : 0;
    const boundarySwipeTransition = boundarySwipeTransitionRef.current;
    const hasBoundarySwipeTransition =
      boundarySwipeTransition?.direction === direction;
    const initialTrackOffset = hasBoundarySwipeTransition
      ? direction === "forward"
        ? boundarySwipeTransition.releasedOffset
        : boundarySwipeTransition.releasedOffset - pageStride
      : direction === "forward"
        ? 0
        : -pageStride;

    boundarySwipeTransitionRef.current = null;

    setOutgoingSnapshot({
      section: prevSectionRef.current,
      page: prevPageRef.current,
      direction,
    });
    setAnimateTrack(false);
    setTrackOffset(initialTrackOffset);

    if (direction === "forward") {
      forcedPageRef.current = null;
    } else {
      const content = contentRef.current;
      const pages =
        content && width > 0 && pageStride > 0
          ? Math.max(
              1,
              Math.ceil((content.scrollWidth + pageGap) / pageStride - 0.01),
            )
          : 1;
      forcedPageRef.current = pages - 1;
    }

    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        setAnimateTrack(true);
        setTrackOffset(targetTrackOffset);

        if (transitionTimeoutRef.current !== null) {
          window.clearTimeout(transitionTimeoutRef.current);
        }
        transitionTimeoutRef.current = window.setTimeout(() => {
          setAnimateTrack(false);
          setOutgoingSnapshot(null);
          transitionTimeoutRef.current = null;
        }, SECTION_TRANSITION_MS);
      });
    });
  }, [
    section,
    sectionIndex,
    pageWidthRef,
    pageStride,
    contentRef,
    boundarySwipeTransitionRef,
  ]);

  // Track previous values for next transition comparison.
  useLayoutEffect(() => {
    prevSectionRef.current = section;
    prevSectionIndexRef.current = sectionIndex;
    prevPageRef.current = currentPage;
  }, [section, sectionIndex, currentPage]);

  // Clean up pending timeout on unmount.
  useLayoutEffect(
    () => () => {
      if (transitionTimeoutRef.current !== null) {
        window.clearTimeout(transitionTimeoutRef.current);
      }
    },
    [],
  );

  return {
    outgoingSnapshot,
    trackOffset,
    animateTrack,
    isSectionTransitioning: outgoingSnapshot !== null,
    forcedPage: forcedPageRef.current,
  };
};
