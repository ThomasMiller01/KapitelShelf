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
            : [
                {
                  label: `Saved · ${absoluteSavedPage}`,
                  value: absoluteSavedPage,
                },
              ]
        }
        size="small"
        max={absoluteTotalPages}
        min={1}
        value={absoluteCurrentPage}
        valueLabelDisplay="auto"
        sx={{
          "& .MuiSlider-mark": {
            width: 4,
            height: 4,
            borderRadius: "50%",
            border: "1.5px solid",
            borderColor: "primary.main",
            backgroundColor: "primary.main",
            opacity: 1,
          },
          "& .MuiSlider-markLabel": {
            fontSize: "0.7rem",
            color: "text.secondary",
            whiteSpace: "nowrap",
            transform: `translateX(-${
              (((absoluteSavedPage ?? 1) - 1) /
                Math.max(absoluteTotalPages - 1, 1)) *
              100
            }%)`,
          },
        }}
        onKeyDown={(event) => event.stopPropagation()}
        onChange={(_, page) => {
          const location = getLocation(page as number);
          onNavigate(location.section, location.page);
        }}
        onChangeCommitted={(event, page) => {
          if (
            event.type.startsWith("key") ||
            absoluteSavedPage === undefined ||
            savedSection === undefined ||
            savedPage === undefined ||
            Math.abs((page as number) - absoluteSavedPage) >
              Math.max(1, (absoluteTotalPages - 1) * 0.01)
          ) {
            return;
          }

          onNavigate(savedSection, savedPage);
        }}
      />
    </Stack>
  );
};
