import { Box, Stack, Typography } from "@mui/material";
import React from "react";

import { RequestErrorCard } from "../../../shared/components/base/feedback/RequestErrorCard";
import { useReaderBattery } from "../hooks/device/useReaderBattery";
import { useReaderClock } from "../hooks/device/useReaderClock";
import { useReaderNavigation } from "../hooks/navigation/useReaderNavigation";
import { useBookPageProgress } from "../hooks/pagination/useBookPageProgress";
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
  currentSection: number;
  currentPage: number;
  savedPage?: number;
  savedSection?: number;
  setLocation: (section: number, page: number) => void;
  setCurrentPage: (page: number) => void;
  setTotalPages: (total: number) => void;
  totalPages: number;
  nextSection: () => void;
  prevSection: () => void;
}

export const Content: React.FC<ContentProps> = ({
  content,
  isCompactLayout,
  controlsVisible,
  onToggleControls,
  currentSection,
  currentPage,
  savedPage,
  savedSection,
  setLocation,
  setCurrentPage,
  setTotalPages,
  totalPages,
  nextSection,
  prevSection,
}) => {
  const { fontScale, contentFontFamily } = useReaderColorScheme();
  const { batteryPercent, isCharging } = useReaderBattery();
  const currentTime = useReaderClock();
  const pageProgress = useBookPageProgress(
    content,
    currentSection,
    currentPage,
    totalPages,
  );
  const {
    absoluteCurrentPage,
    absoluteTotalPages,
    progressPercent,
    onTotalPagesChange: onPageProgressChange,
  } = pageProgress;

  const { handleNext, handlePrev, handleTotalPagesChange } =
    useReaderNavigation({
      currentSection,
      currentPage,
      totalPages,
      setCurrentPage,
      nextSection,
      prevSection,
      setTotalPages,
      onPageProgressChange,
    });

  if (currentSection < 0 || currentSection >= content.sections.length) {
    return (
      <RequestErrorCard
        itemName="Section"
        subtitle="Invalid section, not found."
      />
    );
  }

  const canGoBack = !(currentPage === 0 && currentSection === 0);
  const canGoForward = !(
    currentPage === totalPages - 1 &&
    currentSection === content.sections.length - 1
  );

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
          onClick={handlePrev}
          disabled={!canGoBack}
          direction="prev"
          isCompactLayout={isCompactLayout}
        />
      )}
      <Stack
        direction="column"
        alignItems="center"
        height="100%"
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
          section={content.sections[currentSection]}
          onToggleControls={onToggleControls}
          sectionIndex={currentSection}
          isCompactLayout={isCompactLayout}
          currentPage={currentPage}
          totalPages={totalPages}
          fontScale={fontScale}
          contentFontFamily={contentFontFamily}
          onTotalPagesChange={handleTotalPagesChange}
          onNext={handleNext}
          onPrev={handlePrev}
          canGoBack={canGoBack}
          canGoForward={canGoForward}
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
            setLocation={setLocation}
          />
        </Box>
      )}
      {!isCompactLayout && (
        <PaginationButton
          onClick={handleNext}
          disabled={!canGoForward}
          direction="next"
          isCompactLayout={isCompactLayout}
        />
      )}
    </Stack>
  );
};
