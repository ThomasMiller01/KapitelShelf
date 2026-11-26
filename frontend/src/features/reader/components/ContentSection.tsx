import { Box, useTheme } from "@mui/material";
import { useDrag } from "@use-gesture/react";
import { type ReactElement, useState } from "react";

import { useContainerPagination } from "../hooks/pagination/useContainerPagination";
import type { useReaderPosition } from "../hooks/useReaderPosition";
import type { BookSection } from "../utils/BookContentModels";
import { PaginationButton } from "./PaginationButton";
import { ShadowBookContent } from "./ShadowBookContent";

interface ContentSectionProps {
  section: BookSection;
  position: ReturnType<typeof useReaderPosition>;
  onToggleControls: () => void;
  isCompactLayout: boolean;
  fontScale: number;
  contentFontFamily: string;
  onMeasure: (total: number, resized: boolean) => void;
  onBookLink: (href: string) => void;
}

const PAGE_GAP = 15;

export const ContentSection = ({
  section, position, onToggleControls, isCompactLayout, fontScale, contentFontFamily, onMeasure, onBookLink,
}: ContentSectionProps): ReactElement => {
  const theme = useTheme();
  const { containerRef, contentRef, pageStride } = useContainerPagination(section, fontScale, contentFontFamily, onMeasure, PAGE_GAP);
  const [dragOffset, setDragOffset] = useState(0);
  const swipe = useDrag(({ active, last, canceled, event, movement: [distance], swipe: [direction] }) => {
    if (active) {
      const min = position.canGoForward ? -pageStride : 0;
      const max = position.canGoBack ? pageStride : 0;
      setDragOffset(Math.min(max, Math.max(min, distance)));
    } else if (last) {
      setDragOffset(0);
      if (canceled || event.type.endsWith("cancel") || event.type === "lostpointercapture") {
        return;
      }
      if (direction !== 0 || Math.abs(distance) >= 60) {
        ((direction || distance) < 0 ? position.next : position.previous)();
      }
    }
  }, { axis: "x", filterTaps: true, pointer: { keys: false } });

  return (
    <Box
      ref={containerRef}
      {...swipe()}
      onClick={(event) => {
        const path = event.nativeEvent.composedPath();
        const link = path.find((node): node is HTMLAnchorElement => node instanceof HTMLAnchorElement);
        const href = link?.getAttribute("href");
        if (link?.dataset.bookLink) {
          event.preventDefault();
          onBookLink(link.dataset.bookLink);
        } else if (link && href?.startsWith("#")) {
          event.preventDefault();
          const id = href.slice(1);
          const root = link.getRootNode();
          const columns = contentRef.current;
          const target = root instanceof ShadowRoot ? root.getElementById(id) : null;
          if (target && columns && pageStride > 0) {
            const offset = target.getBoundingClientRect().left - columns.getBoundingClientRect().left;
            position.jump(section.index, Math.floor(offset / pageStride));
          }
        } else if (!path.some((node) => node instanceof Element && node.matches("button, a, input, select, textarea"))) {
          onToggleControls();
        }
      }}
      tabIndex={0}
      role="region"
      aria-label="Reading page; press Enter or Space to toggle reader controls"
      onKeyDown={(event) => {
        if (event.target === event.currentTarget && (event.key === "Enter" || event.key === " ")) {
          event.preventDefault();
          onToggleControls();
        }
      }}
      sx={{
        aspectRatio: isCompactLayout ? "none" : "2 / 3",
        maxWidth: "100%",
        maxHeight: "100%",
        height: "100%",
        width: "100%",
        position: "relative",
        overflow: "hidden",
        background: theme.palette.background.paper,
        borderRadius: 1,
        py: isCompactLayout ? 1 : 2,
        mt: "5px !important",
        touchAction: "pan-y",
      }}
    >
      {isCompactLayout && (
        <>
          <PaginationButton onClick={position.previous} disabled={!position.canGoBack} direction="prev" isCompactLayout />
          <PaginationButton onClick={position.next} disabled={!position.canGoForward} direction="next" isCompactLayout />
        </>
      )}
      <Box sx={{ height: "100%", overflow: "hidden", px: isCompactLayout ? 1.5 : 0 }}>
        <Box ref={contentRef} style={{ transform: `translateX(${-position.page * pageStride + dragOffset}px)` }} sx={{
          height: "100%",
          columns: "1",
          columnFill: "auto",
          columnGap: `${PAGE_GAP}px`,
          visibility: position.pageCount === null ? "hidden" : "visible",
        }}>
          <Box sx={{ px: isCompactLayout ? 1.5 : 3, height: "100%" }}>
            <ShadowBookContent html={section.content} theme={theme} fontScale={fontScale} contentFontFamily={contentFontFamily} />
          </Box>
        </Box>
      </Box>
    </Box>
  );
};
