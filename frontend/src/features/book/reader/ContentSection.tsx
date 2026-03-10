import { Box, useTheme } from "@mui/material";
import React from "react";

import { useMobile } from "../../../hooks/useMobile";
import type { BookSection } from "../../../utils/bookReader/BookContent";
import { ShadowBookContent } from "./ShadowBookContent";
import { useContainerPagination } from "./useContainerPagination";
import { useSectionTransition } from "./useSectionTransition";

interface ContentSectionProps {
  section: BookSection;
  sectionIndex: number;
  currentPage: number;
  fontScale: number;
  onTotalPagesChange: (total: number) => void;
}

const PAGE_TRANSITION = "transform 0.15s cubic-bezier(0.25, 0.46, 0.45, 0.94)";

export const ContentSection: React.FC<ContentSectionProps> = ({
  section,
  sectionIndex,
  currentPage,
  fontScale,
  onTotalPagesChange,
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

  const renderSectionContent = (
    targetSection: BookSection,
    page: number,
    options?: {
      attachContentRef?: boolean;
      animatePage?: boolean;
    },
  ) => (
    <Box sx={{ height: "100%", overflow: "hidden" }}>
      <Box
        ref={options?.attachContentRef ? contentRef : undefined}
        sx={{
          height: "100%",
          columns: "1",
          columnFill: "auto",
          columnGap: 0,
          transform: `translateX(${-page * containerWidth}px)`,
          transition: options?.animatePage ? PAGE_TRANSITION : "none",
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
      sx={{
        aspectRatio: isMobile ? "none" : "2 / 3",
        maxWidth: "100%",
        maxHeight: "100%",
        height: "100%",
        overflow: "hidden",
        background: theme.palette.background.paper,
        borderRadius: 1,
        py: isMobile ? 1 : 2,
        mt: "5px !important",
      }}
    >
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
        })
      )}
    </Box>
  );
};
