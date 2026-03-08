import type { Theme } from "@mui/material";
import { Box, useTheme } from "@mui/material";
import React, { useLayoutEffect, useRef, useState } from "react";

import { useMobile } from "../../../hooks/useMobile";
import type { BookSection } from "../../../utils/bookReader/BookContent";

interface ContentSectionProps {
  section: BookSection;
  sectionIndex: number;
  currentPage: number;
  onTotalPagesChange: (total: number) => void;
}

type TransitionDirection = "forward" | "backward";

interface OutgoingSectionSnapshot {
  section: BookSection;
  page: number;
  direction: TransitionDirection;
}

const PAGE_TRANSITION = "transform 0.15s cubic-bezier(0.25, 0.46, 0.45, 0.94)";
const SECTION_TRANSITION_MS = 180;

export const ContentSection: React.FC<ContentSectionProps> = ({
  section,
  sectionIndex,
  currentPage,
  onTotalPagesChange,
}) => {
  const theme = useTheme();
  const containerRef = useRef<HTMLDivElement>(null);
  const contentRef = useRef<HTMLDivElement>(null);
  const [containerWidth, setContainerWidth] = useState(0);
  const { isMobile } = useMobile();

  // Track previously committed values; used to build a transition snapshot.
  const prevSectionRef = useRef(section);
  const prevSectionIndexRef = useRef(sectionIndex);
  const prevPageRef = useRef(currentPage);

  // Mirror of containerWidth as a ref so layout effects can read it without re-running.
  const containerWidthRef = useRef(0);

  // Backward navigation page fix: URL page update is async, so temporarily override page.
  const forcedPageRef = useRef<number | null>(null);
  const transitionTimeoutRef = useRef<number | null>(null);
  const [outgoingSnapshot, setOutgoingSnapshot] =
    useState<OutgoingSectionSnapshot | null>(null);
  const [trackOffset, setTrackOffset] = useState(0);
  const [animateTrack, setAnimateTrack] = useState(false);

  // Use forcedPageRef while the URL hasn't updated yet; falls back to the real prop.
  const effectivePage = forcedPageRef.current ?? currentPage;
  const isSectionTransitioning = outgoingSnapshot !== null;
  const animatePageFlip = !isSectionTransitioning && containerWidth > 0;

  // Once currentPage prop matches the forced value, the URL has caught up, clear the override.
  useLayoutEffect(() => {
    if (
      forcedPageRef.current !== null &&
      currentPage === forcedPageRef.current
    ) {
      forcedPageRef.current = null;
    }
  }, [currentPage]);

  useLayoutEffect(() => {
    const container = containerRef.current;
    const content = contentRef.current;
    if (!container || !content) {
      return;
    }

    const measure = () => {
      const { width } = content.getBoundingClientRect();
      setContainerWidth(width);
      containerWidthRef.current = width;
      const pages =
        width > 0 ? Math.ceil(content.scrollWidth / width - 0.01) : 1;
      onTotalPagesChange(Math.max(1, pages));
    };

    measure();

    const observer = new ResizeObserver(measure);
    observer.observe(container);
    return () => observer.disconnect();
  }, [section, onTotalPagesChange]);

  // Keep outgoing section mounted while sliding between sections.
  useLayoutEffect(() => {
    if (
      section === prevSectionRef.current ||
      sectionIndex === prevSectionIndexRef.current ||
      containerWidthRef.current === 0
    ) {
      return;
    }

    const direction: TransitionDirection =
      sectionIndex > prevSectionIndexRef.current ? "forward" : "backward";
    const width = containerWidthRef.current;
    const initialTrackOffset = direction === "forward" ? 0 : -width;
    const targetTrackOffset = direction === "forward" ? -width : 0;

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
        content && width > 0
          ? Math.max(1, Math.ceil(content.scrollWidth / width - 0.01))
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
  }, [section, sectionIndex]);

  useLayoutEffect(() => {
    prevSectionRef.current = section;
    prevSectionIndexRef.current = sectionIndex;
    prevPageRef.current = currentPage;
  }, [section, sectionIndex, currentPage]);

  useLayoutEffect(
    () => () => {
      if (transitionTimeoutRef.current !== null) {
        window.clearTimeout(transitionTimeoutRef.current);
      }
    },
    [],
  );

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
            ...ContentStyles(theme),
          }}
          dangerouslySetInnerHTML={{ __html: targetSection.content || "" }}
        />
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

const ContentStyles = (theme: Theme): any => ({
  "&": {
    color: `${theme.palette.text.primary} !important`,
    background: "transparent",
    fontFamily: theme.typography.fontFamily,
    fontSize: "1rem",
    lineHeight: 1.7,
    wordBreak: "break-word",
    overflowWrap: "break-word",
  },

  // Override any EPUB-authored forced page/column breaks that cause empty pages
  "& *": {
    breakAfter: "auto !important",
    breakBefore: "auto !important",
    pageBreakAfter: "auto !important",
    pageBreakBefore: "auto !important",
    breakInside: "auto !important",
    pageBreakInside: "auto !important",
  },

  "& p": {
    margin: "0 0 0.5em 0 !important",
    color: `${theme.palette.text.primary} !important`,
    fontFamily: `${theme.typography.fontFamily} !important`,
    textAlign: "justify",
    textAlignLast: "left",
  },

  "& a": {
    color: `${theme.palette.primary.main} !important`,
    textDecoration: "underline",
    textDecorationColor: theme.palette.primary.main,
    "&:hover": {
      color: theme.palette.primary.dark,
      textDecorationColor: theme.palette.primary.dark,
    },
    "&:visited": {
      color: theme.palette.primary.main,
    },
  },

  "& img": {
    maxWidth: "100%",
    height: "auto",
    display: "block",
  },

  "& blockquote": {
    borderLeft: `4px solid ${theme.palette.divider}`,
    paddingLeft: "1em",
    marginLeft: 0,
    color: theme.palette.text.secondary,
    fontStyle: "italic",
  },

  "& ul": {
    paddingLeft: "1.6em",
    marginBottom: "1em",
  },

  "& ol": {
    paddingLeft: "1.6em",
    marginBottom: "1em",
  },

  "& li": {
    marginBottom: "0.4em",
  },

  "& pre": {
    overflowX: "auto",
    padding: "12px",
    borderRadius: "6px",
    background: theme.palette.background.paper,
    fontFamily: "monospace",
  },

  "& code": {
    fontFamily: "monospace",
    background: theme.palette.action.hover,
    padding: "2px 4px",
    borderRadius: "4px",
  },

  "& hr": {
    border: "none",
    borderTop: `1px solid ${theme.palette.divider}`,
    margin: "2em 0",
  },
});
