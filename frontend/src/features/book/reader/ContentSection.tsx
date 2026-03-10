import { Box, useTheme } from "@mui/material";
import React from "react";

import { useMobile } from "../../../hooks/useMobile";
import type { BookSection } from "../../../utils/bookReader/BookContent";
import { PaginationButton } from "./PaginationButton";
import { ShadowBookContent } from "./ShadowBookContent";
import { useContainerPagination } from "./useContainerPagination";
import { useSectionTransition } from "./useSectionTransition";
import { useSwipeNavigation } from "./useSwipeNavigation";

interface ContentSectionProps {
  section: BookSection;
  sectionIndex: number;
  currentPage: number;
  fontScale: number;
  onTotalPagesChange: (total: number) => void;
  onNext: () => void;
  onPrev: () => void;
  canGoBack: boolean;
  canGoForward: boolean;
}

const PAGE_TRANSITION = "transform 0.09s cubic-bezier(0.25, 0.46, 0.45, 0.94)";

export const ContentSection: React.FC<ContentSectionProps> = ({
  section,
  sectionIndex,
  currentPage,
  fontScale,
  onTotalPagesChange,
  onNext,
  onPrev,
  canGoBack,
  canGoForward,
}) => {
  const theme = useTheme();
  const { isMobile } = useMobile();

  const { containerRef, contentRef, containerWidth, containerWidthRef } =
    useContainerPagination({ fontScale, section, onTotalPagesChange });

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
    containerWidthRef,
    contentRef,
  });

  const effectivePage = forcedPage ?? currentPage;
  const animatePageFlip = !isSectionTransitioning && containerWidth > 0;

  const {
    dragOffset,
    isSwiping,
    isSnapping,
    onTransitionEnd,
    bindSwipe,
  } = useSwipeNavigation({
    containerWidth,
    effectivePage,
    canGoBack,
    canGoForward,
    isSectionTransitioning,
    onNext,
    onPrev,
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
    <Box sx={{ height: "100%", overflow: "hidden" }}>
      <Box
        ref={options?.attachContentRef ? contentRef : undefined}
        onTransitionEnd={options?.attachContentRef ? onTransitionEnd : undefined}
        sx={{
          height: "100%",
          columns: "1",
          columnFill: "auto",
          columnGap: 0,
          transform: `translateX(${-page * containerWidth + (options?.applyDragOffset ? dragOffset : 0)}px)`,
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
