import { Box, Stack, Typography } from "@mui/material";
import { type ReactElement, useCallback } from "react";

import { useReaderBattery } from "../hooks/device/useReaderBattery";
import { useReaderClock } from "../hooks/device/useReaderClock";
import { useBookPageProgress } from "../hooks/pagination/useBookPageProgress";
import type { useReaderPosition } from "../hooks/useReaderPosition";
import type { BookContent } from "../utils/BookContentModels";
import { BatteryStatus } from "./BatteryStatus";
import { BottomControls } from "./BottomControls";
import { ContentSection } from "./ContentSection";
import { PaginationButton } from "./PaginationButton";
import { useReaderColorScheme } from "./ThemeProvider";

interface ContentProps {
  content: BookContent;
  isCompactLayout: boolean;
  controlsVisible: boolean;
  onToggleControls: () => void;
  position: ReturnType<typeof useReaderPosition>;
  savedPage?: number;
  savedSection?: number;
}

export const Content = ({
  content,
  isCompactLayout,
  controlsVisible,
  onToggleControls,
  position,
  savedPage,
  savedSection,
}: ContentProps): ReactElement => {
  const { section, page, pageCount } = position;
  const { measure } = position;
  const { fontScale, contentFontFamily } = useReaderColorScheme();
  const { batteryPercent, isCharging } = useReaderBattery();
  const currentTime = useReaderClock();
  const pageProgress = useBookPageProgress(
    content,
    section,
    page,
    pageCount ?? 1,
  );
  const {
    absoluteCurrentPage,
    absoluteTotalPages,
    progressPercent,
    onTotalPagesChange: onPageProgressChange,
  } = pageProgress;

  const onMeasure = useCallback((pages: number, resized: boolean): void => {
    onPageProgressChange(pages, resized);
    measure(section, pages);
  }, [measure, onPageProgressChange, section]);

  const openBookLink = (href: string): void => {
    const current = content.sections[section];
    const source = current.href ?? content.sections.find((item) => item.sourceSectionIndex === current.sourceSectionIndex)?.href;
    const root = "https://book.invalid/";
    const destination = new URL(`./${href}`, new URL(`./${source ?? ""}`, root)).pathname;
    const target = content.sections.find((item) => item.href && new URL(`./${item.href}`, root).pathname === destination);
    if (target) {
      position.jump(target.index, 0);
    }
  };

  return (
    <Stack
      px={isCompactLayout ? 0 : 4}
      pt={isCompactLayout ? 2.5 : 4}
      pb={isCompactLayout ? 1.5 : 4}
      height="100%"
      maxWidth="100%"
      justifyContent="center"
      alignItems="center"
      direction="row"
      position="relative"
      bgcolor={
        isCompactLayout && !controlsVisible
          ? "background.paper"
          : "background.default"
      }
    >
      {!isCompactLayout && (
        <PaginationButton
          onClick={position.previous}
          disabled={!position.canGoBack}
          direction="prev"
          isCompactLayout={isCompactLayout}
        />
      )}
      <Stack
        direction="column"
        alignItems="center"
        height="100%"
        minWidth={0}
        width={isCompactLayout ? "100%" : "auto"}
        justifyContent="center"
        spacing={1}
        sx={{
          transform: controlsVisible ? "scale(0.78)" : "none",
          transition: "transform 0.2s ease",
        }}
      >
        <Stack
          direction="row"
          justifyContent="space-between"
          alignItems="center"
          width="100%"
          px={isCompactLayout ? 2.5 : 3.5}
        >
          <Typography
            variant="caption"
            color="text.secondary"
            sx={{ fontSize: `${0.75 * fontScale}rem` }}
          >
            {currentTime}
          </Typography>
          <BatteryStatus
            batteryPercent={batteryPercent}
            isCharging={isCharging}
            fontScale={fontScale}
          />
        </Stack>
        <ContentSection
          section={content.sections[section]}
          position={position}
          onToggleControls={onToggleControls}
          isCompactLayout={isCompactLayout}
          fontScale={fontScale}
          contentFontFamily={contentFontFamily}
          onMeasure={onMeasure}
          onBookLink={openBookLink}
        />
        <Stack
          direction="row"
          spacing={2}
          justifyContent="space-between"
          width="100%"
          px={isCompactLayout ? 2.5 : 3.5}
        >
          <Typography
            variant="caption"
            color="text.secondary"
            sx={{ fontSize: `${0.75 * fontScale}rem` }}
          >
            Page {absoluteCurrentPage} of {absoluteTotalPages}
          </Typography>
          <Typography
            variant="caption"
            color="text.secondary"
            sx={{ fontSize: `${0.75 * fontScale}rem` }}
          >
            {progressPercent}%
          </Typography>
        </Stack>
      </Stack>
      {controlsVisible && (
        <Box position="absolute" bottom={0} left={0} width="100%">
          <BottomControls
            pageProgress={pageProgress}
            savedPage={savedPage}
            savedSection={savedSection}
            setLocation={position.jump}
          />
        </Box>
      )}
      {!isCompactLayout && (
        <PaginationButton
          onClick={position.next}
          disabled={!position.canGoForward}
          direction="next"
          isCompactLayout={isCompactLayout}
        />
      )}
    </Stack>
  );
};
