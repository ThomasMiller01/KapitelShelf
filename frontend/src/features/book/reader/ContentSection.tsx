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
          }}
        >
          <ShadowBookContent html={targetSection.content || ""} theme={theme} />
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

interface ShadowBookContentProps {
  html: string;
  theme: Theme;
}

const ShadowBookContent: React.FC<ShadowBookContentProps> = ({
  html,
  theme,
}) => {
  const hostRef = useRef<HTMLDivElement>(null);

  useLayoutEffect(() => {
    const host = hostRef.current;
    if (!host) {
      return;
    }

    const shadowRoot = host.shadowRoot ?? host.attachShadow({ mode: "open" });
    shadowRoot.innerHTML = `<style>${ContentStyles(theme)}</style>${html}`;
  }, [html, theme]);

  return <Box ref={hostRef} sx={{ height: "100%" }} />;
};

const ContentStyles = (theme: Theme): string => `
:host {
  color: ${theme.palette.text.primary} !important;
  background: transparent;
  font-family: ${theme.typography.fontFamily};
  font-size: 1rem;
  line-height: 1.7;
  word-break: break-word;
  overflow-wrap: break-word;
  display: block;
  height: 100%;
}

:host * {
  break-after: auto !important;
  break-before: auto !important;
  page-break-after: auto !important;
  page-break-before: auto !important;
  break-inside: auto !important;
  page-break-inside: auto !important;
}

:host p {
  margin: 0 0 0.5em 0 !important;
  color: ${theme.palette.text.primary} !important;
  font-family: ${theme.typography.fontFamily} !important;
  text-align: justify;
  text-align-last: left;
}

:host a {
  color: ${theme.palette.primary.main} !important;
  text-decoration: underline;
  text-decoration-color: ${theme.palette.primary.main};
}

:host a:hover {
  color: ${theme.palette.primary.dark};
  text-decoration-color: ${theme.palette.primary.dark};
}

:host a:visited {
  color: ${theme.palette.primary.main};
}

:host img {
  max-width: 100%;
  height: auto;
  display: block;
}

:host blockquote {
  border-left: 4px solid ${theme.palette.divider};
  padding-left: 1em;
  margin-left: 0;
  color: ${theme.palette.text.secondary};
  font-style: italic;
}

:host ul,
:host ol {
  padding-left: 1.6em;
  margin-bottom: 1em;
}

:host li {
  margin-bottom: 0.4em;
}

:host pre {
  overflow-x: auto;
  padding: 12px;
  border-radius: 6px;
  background: ${theme.palette.background.paper};
  font-family: monospace;
}

:host code {
  font-family: monospace;
  background: ${theme.palette.action.hover};
  padding: 2px 4px;
  border-radius: 4px;
}

:host hr {
  border: none;
  border-top: 1px solid ${theme.palette.divider};
  margin: 2em 0;
}
`;
