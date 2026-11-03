import { Slider, Stack } from "@mui/material";

import type { useBookPageProgress } from "../hooks/pagination/useBookPageProgress";

interface TimelineProps {
  pageProgress: ReturnType<typeof useBookPageProgress>;
  onNavigate: (section: number, page: number) => void;
  savedPage?: number;
  savedSection?: number;
}

export const Timeline: React.FC<TimelineProps> = ({
  pageProgress,
  onNavigate,
  savedPage,
  savedSection,
}) => {
  const {
    absoluteCurrentPage,
    absoluteTotalPages,
    getAbsolutePage,
    getLocation,
  } = pageProgress;

  const absoluteSavedPage =
    savedSection === undefined || savedPage === undefined
      ? undefined
      : getAbsolutePage(savedSection, savedPage);

  return (
    <Stack px={2.5} pt={{ xs: 0, md: 1 }} pb={1} borderRadius={1}>
      <Slider
        aria-label="Reading position"
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
