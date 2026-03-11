import { Stack, Typography } from "@mui/material";
import React, { useState } from "react";
import { RequestErrorCard } from "../../../components/base/feedback/RequestErrorCard";
import { BatteryStatus } from "../../../components/reader/BatteryStatus";
import { useMobile } from "../../../hooks/useMobile";
import type { BookContent } from "../../../utils/bookReader/BookContent";
import { ContentSection } from "./ContentSection";
import { PaginationButton } from "./PaginationButton";
import { useReaderColorScheme } from "./ThemeProvider";
import { useBookPageProgress } from "./useBookPageProgress";
import { useReaderBattery } from "./useReaderBattery";
import { useReaderClock } from "./useReaderClock";
import { useReaderNavigation } from "./useReaderNavigation";

interface ContentProps {
  content: BookContent;
  currentSection: number;
  currentPage: number;
  setCurrentPage: (page: number) => void;
  nextSection: () => void;
  prevSection: () => void;
}

export const Content: React.FC<ContentProps> = ({
  content,
  currentSection,
  currentPage,
  setCurrentPage,
  nextSection,
  prevSection,
}) => {
  const { isMobile } = useMobile();
  const { fontScale } = useReaderColorScheme();
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
      px={isMobile ? 0 : 4}
      pt={isMobile ? 2.5 : 4}
      pb={isMobile ? 1.5 : 4}
      height="100%"
      maxWidth="100%"
      justifyContent="center"
      alignItems="center"
      direction="row"
      position="relative"
      bgcolor={isMobile ? "background.paper" : "background.default"}
    >
      {!isMobile && (
        <PaginationButton
          onClick={handlePrev}
          disabled={!canGoBack}
          direction="prev"
        />
      )}
      <Stack
        direction="column"
        alignItems="center"
        height="100%"
        width={{ xs: "100%", sm: "auto" }}
        justifyContent="center"
        spacing={1}
      >
        <Stack
          direction="row"
          justifyContent="space-between"
          alignItems="center"
          width="100%"
          px={isMobile ? 2.5 : 3.5}
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
          currentPage={currentPage}
          totalPages={totalPages}
          fontScale={fontScale}
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
          px={isMobile ? 2.5 : 3.5}
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
      {!isMobile && (
        <PaginationButton
          onClick={handleNext}
          disabled={!canGoForward}
          direction="next"
        />
      )}
    </Stack>
  );
};
