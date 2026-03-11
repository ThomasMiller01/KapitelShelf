import { Box, useTheme } from "@mui/material";
import React, { useRef } from "react";

import { useMobile } from "../../../hooks/useMobile";
import type { BookSection } from "../../../utils/bookReader/BookContent";
import { PaginationButton } from "./PaginationButton";
import { ShadowBookContent } from "./ShadowBookContent";
import { useContainerPagination } from "./useContainerPagination";
import { useSectionTransition } from "./useSectionTransition";
import {
  useSwipeNavigation,
  type BoundarySwipeTransition,
} from "./useSwipeNavigation";

interface ContentSectionProps {
  section: BookSection;
  sectionIndex: number;
  currentPage: number;
  totalPages: number;
  fontScale: number;
  onTotalPagesChange: (total: number) => void;
  onNext: () => void;
  onPrev: () => void;
  canGoBack: boolean;
  canGoForward: boolean;
}

const PAGE_TRANSITION = "transform 0.09s cubic-bezier(0.25, 0.46, 0.45, 0.94)";
const MOBILE_PAGE_GAP = 15;

export const ContentSection: React.FC<ContentSectionProps> = ({
  section,
  sectionIndex,
  currentPage,
  totalPages,
  fontScale,
  onTotalPagesChange,
  onNext,
  onPrev,
  canGoBack,
  canGoForward,
}) => {
  const theme = useTheme();
  const { isMobile } = useMobile();
  const boundarySwipeTransitionRef = useRef<BoundarySwipeTransition | null>(
    null,
  );

  const { containerRef, contentRef, pageWidth, pageWidthRef, pageStride } =
    useContainerPagination({
      fontScale,
      section,
      onTotalPagesChange,
      pageGap: MOBILE_PAGE_GAP,
    });

  const {
    outgoingSnapshot,
    trackOffset,
    animateTrack,
    isSectionTransitioning,
    forcedPage,
  } = useSectionTransition({
    section,
    sectionIndex,
    currentPage,
    pageWidthRef,
    pageStride,
    contentRef,
    boundarySwipeTransitionRef,
  });

  const effectivePage = forcedPage ?? currentPage;
  const animatePageFlip = !isSectionTransitioning && pageWidth > 0;
  const isAtSectionStart = currentPage === 0;
  const isAtSectionEnd = currentPage === totalPages - 1;

  const { dragOffset, isSwiping, isSnapping, onTransitionEnd, bindSwipe } =
    useSwipeNavigation({
      pageWidth,
      pageStride,
      effectivePage,
      isAtSectionStart,
      isAtSectionEnd,
      canGoBack,
      canGoForward,
      isSectionTransitioning,
      onNext,
      onPrev,
      onBoundarySwipeCommit: (transition) => {
        boundarySwipeTransitionRef.current = transition;
      },
    });

  // Determine CSS transition for the content transform.
  const getContentTransition = (animatePage?: boolean): string => {
    if (isSwiping) {
      return "none";
    }

    if (isSnapping) {
      return PAGE_TRANSITION;
    }

    return animatePage ? PAGE_TRANSITION : "none";
  };

  const renderSectionContent = (
    targetSection: BookSection,
    page: number,
    options?: {
      attachContentRef?: boolean;
      animatePage?: boolean;
      applyDragOffset?: boolean;
    },
  ) => (
    <Box
      sx={{
        height: "100%",
        overflow: "hidden",
        px: isMobile ? 1.5 : 0,
      }}
    >
      <Box
        ref={options?.attachContentRef ? contentRef : undefined}
        onTransitionEnd={
          options?.attachContentRef ? onTransitionEnd : undefined
        }
        sx={{
          height: "100%",
          columns: "1",
          columnFill: "auto",
          columnGap: `${MOBILE_PAGE_GAP}px`,
          transform: `translateX(${
            -page * pageStride + (options?.applyDragOffset ? dragOffset : 0)
          }px)`,
          transition: getContentTransition(options?.animatePage),
        }}
      >
        <Box
          sx={{
            px: isMobile ? 1.5 : 3,
            height: "100%",
          }}
        >
          <ShadowBookContent
            html={targetSection.content || ""}
            theme={theme}
            fontScale={fontScale}
          />
        </Box>
      </Box>
    </Box>
  );

  let transitionPanels: Array<{
    key: "incoming" | "outgoing";
    section: BookSection;
    page: number;
    attachContentRef: boolean;
  }> = [];

  if (outgoingSnapshot) {
    if (outgoingSnapshot.direction === "forward") {
      transitionPanels = [
        {
          key: "outgoing",
          section: outgoingSnapshot.section,
          page: outgoingSnapshot.page,
          attachContentRef: false,
        },
        {
          key: "incoming",
          section,
          page: effectivePage,
          attachContentRef: true,
        },
      ];
    } else {
      transitionPanels = [
        {
          key: "incoming",
          section,
          page: effectivePage,
          attachContentRef: true,
        },
        {
          key: "outgoing",
          section: outgoingSnapshot.section,
          page: outgoingSnapshot.page,
          attachContentRef: false,
        },
      ];
    }
  }

  return (
    <Box
      ref={containerRef}
      {...bindSwipe()}
      sx={{
        aspectRatio: isMobile ? "none" : "2 / 3",
        maxWidth: "100%",
        maxHeight: "100%",
        height: "100%",
        width: "100%",
        position: "relative",
        overflow: "hidden",
        background: theme.palette.background.paper,
        borderRadius: 1,
        py: isMobile ? 1 : 2,
        mt: "5px !important",
        touchAction: "pan-y",
      }}
    >
      {isMobile && (
        <>
          <PaginationButton
            onClick={onPrev}
            disabled={!canGoBack}
            direction="prev"
          />
          <PaginationButton
            onClick={onNext}
            disabled={!canGoForward}
            direction="next"
          />
        </>
      )}
      {isSectionTransitioning ? (
        <Box
          sx={{
            height: "100%",
            display: "flex",
            gap: `${MOBILE_PAGE_GAP}px`,
            transform: `translateX(${trackOffset}px)`,
            transition: animateTrack ? PAGE_TRANSITION : "none",
          }}
        >
          {transitionPanels.map((panel) => (
            <Box key={panel.key} sx={{ height: "100%", flex: "0 0 100%" }}>
              {renderSectionContent(panel.section, panel.page, {
                attachContentRef: panel.attachContentRef,
                animatePage: false,
              })}
            </Box>
          ))}
        </Box>
      ) : (
        renderSectionContent(section, effectivePage, {
          attachContentRef: true,
          animatePage: animatePageFlip,
          applyDragOffset: true,
        })
      )}
    </Box>
  );
};
