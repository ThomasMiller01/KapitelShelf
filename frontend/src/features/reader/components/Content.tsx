import { Stack, Typography } from "@mui/material";
import React, { useState } from "react";
import { RequestErrorCard } from "../../../shared/components/base/feedback/RequestErrorCard";
import { BatteryStatus } from "./BatteryStatus";
import type { BookContent } from "../utils/BookContentModels";
import { useReaderBattery } from "../hooks/device/useReaderBattery";
import { useReaderClock } from "../hooks/device/useReaderClock";
import { useReaderNavigation } from "../hooks/navigation/useReaderNavigation";
import { useBookPageProgress } from "../hooks/pagination/useBookPageProgress";
import { ContentSection } from "./ContentSection";
import { PaginationButton } from "./PaginationButton";
import { useReaderColorScheme } from "./ThemeProvider";

interface ContentProps {
  content: BookContent;
  isCompactLayout: boolean;
  currentSection: number;
  currentPage: number;
  setCurrentPage: (page: number) => void;
  nextSection: () => void;
  prevSection: () => void;
}

export const Content: React.FC<ContentProps> = ({
  content,
  isCompactLayout,
  currentSection,
  currentPage,
  setCurrentPage,
  nextSection,
  prevSection,
}) => {
  const { fontScale, contentFontFamily } = useReaderColorScheme();
  const { batteryPercent, isCharging } = useReaderBattery();
  const currentTime = useReaderClock();
  const [totalPages, setTotalPages] = useState(1);

  const {
    absoluteCurrentPage,
    absoluteTotalPages,
    progressPercent,
    onTotalPagesChange: onPageProgressChange,
  } = useBookPageProgress(content, currentSection, currentPage, totalPages);

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
      bgcolor={isCompactLayout ? "background.paper" : "background.default"}
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
