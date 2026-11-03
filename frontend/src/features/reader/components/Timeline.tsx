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
    <Stack px={2.5} py={1} bgcolor="background.paper" borderRadius={1}>
      <Slider
        marks={
          absoluteSavedPage === undefined
            ? undefined
            : [{ label: absoluteSavedPage, value: absoluteSavedPage }]
        }
        size="small"
        max={absoluteTotalPages}
        min={1}
        value={absoluteCurrentPage}
        valueLabelDisplay="auto"
        onKeyDown={(event) => event.stopPropagation()}
        onChange={(_, page) => {
          const location = getLocation(page as number);
          onNavigate(location.section, location.page);
        }}
      />
    </Stack>
  );
};
