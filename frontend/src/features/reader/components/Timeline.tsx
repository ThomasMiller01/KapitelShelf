import { Slider, Stack } from "@mui/material";
import { useEffect } from "react";

import { useBookPageProgress } from "../hooks/pagination/useBookPageProgress";
import type { BookContent } from "../utils/BookContentModels";

interface TimelineProps {
  content: BookContent;
  currentPage: number;
  currentSection: number;
  onNavigate: (section: number, page: number) => void;
  savedPage?: number;
  savedSection?: number;
  totalPages: number;
}

export const Timeline: React.FC<TimelineProps> = ({
  content,
  currentPage,
  currentSection,
  onNavigate,
  savedPage,
  savedSection,
  totalPages,
}) => {
  const {
    absoluteCurrentPage,
    absoluteTotalPages,
    getAbsolutePage,
    getLocation,
    onTotalPagesChange,
  } = useBookPageProgress(content, currentSection, currentPage, totalPages);

  useEffect(() => {
    onTotalPagesChange(totalPages);
  }, [onTotalPagesChange, totalPages]);

  const absoluteSavedPage =
    savedSection === undefined || savedPage === undefined
      ? undefined
      : getAbsolutePage(savedSection, savedPage);

  return (
    <Stack px={2.5} py={2} bgcolor="background.paper">
      <Slider
        aria-label="Reading position"
        marks={
          absoluteSavedPage === undefined
            ? undefined
            : [{ label: "Last saved", value: absoluteSavedPage }]
        }
        max={absoluteTotalPages}
        min={1}
        value={absoluteCurrentPage}
        valueLabelDisplay="auto"
        onChangeCommitted={(_, page) => {
          const location = getLocation(page as number);
          onNavigate(location.section, location.page);
        }}
      />
    </Stack>
  );
};
