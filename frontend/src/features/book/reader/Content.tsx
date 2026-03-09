import NavigateBeforeIcon from "@mui/icons-material/NavigateBefore";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";
import { IconButton, Stack, Typography } from "@mui/material";
import React, { useCallback, useEffect, useRef, useState } from "react";
import { RequestErrorCard } from "../../../components/base/feedback/RequestErrorCard";
import { useMobile } from "../../../hooks/useMobile";
import type { BookContent } from "../../../utils/bookReader/BookContent";
import { ContentSection } from "./ContentSection";
import { useReaderColorScheme } from "./ThemeProvider";
import { useBookPageProgress } from "./useBookPageProgress";

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
  const [currentTime, setCurrentTime] = useState(() =>
    new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }),
  );

  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentTime(
        new Date().toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        }),
      );
    }, 10000);
    return () => clearInterval(interval);
  }, []);
  const [totalPages, setTotalPages] = useState(1);
  const navigatedBackRef = useRef(false);
  const isInitialMountRef = useRef(true);

  useEffect(() => {
    if (isInitialMountRef.current) {
      isInitialMountRef.current = false;
      return; // Preserve page from URL on initial load
    }

    if (navigatedBackRef.current) {
      navigatedBackRef.current = false;
      return; // handleTotalPagesChange already set the correct last page
    }
  }, [currentSection]);

  const {
    absoluteCurrentPage,
    absoluteTotalPages,
    progressPercent,
    onTotalPagesChange: onPageProgressChange,
  } = useBookPageProgress(content, currentSection, currentPage, totalPages);

  const handleTotalPagesChange = useCallback(
    (total: number) => {
      setTotalPages(total);
      onPageProgressChange(total);

      if (navigatedBackRef.current) {
        // Don't clear the flag here, let useEffect do it after layout effects settle
        setCurrentPage(total - 1);
      } else if (currentPage > total - 1) {
        setCurrentPage(total - 1);
      }
    },
    [currentPage, onPageProgressChange, setCurrentPage],
  );

  const handleNext = () => {
    if (currentPage < totalPages - 1) {
      setCurrentPage(currentPage + 1);
    } else {
      nextSection();
    }
  };

  const handlePrev = () => {
    if (currentPage > 0) {
      setCurrentPage(currentPage - 1);
    } else {
      navigatedBackRef.current = true;
      prevSection();
    }
  };

  useEffect(() => {
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "ArrowRight") {
        handleNext();
      } else if (e.key === "ArrowLeft") {
        handlePrev();
      }
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  });

  if (currentSection < 0 || currentSection >= content.sections.length) {
    return (
      <RequestErrorCard
        itemName="Section"
        subtitle="Invalid section, not found."
      />
    );
  }

  return (
    <Stack
      px={isMobile ? 1.5 : 4}
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
      <PaginationButton
        onClick={handlePrev}
        disabled={currentPage === 0 && currentSection === 0}
        direction="prev"
      />
      <Stack
        direction="column"
        alignItems="center"
        height="100%"
        width={{ xs: "100%", sm: "auto" }}
        justifyContent="center"
        spacing={1}
      >
        <Typography
          variant="caption"
          color="text.disabled"
          alignSelf="flex-start"
          px={isMobile ? 1 : 2.5}
          sx={{ fontSize: `${0.75 * fontScale}rem` }}
        >
          {currentTime}
        </Typography>
        <ContentSection
          section={content.sections[currentSection]}
          sectionIndex={currentSection}
          currentPage={currentPage}
          fontScale={fontScale}
          onTotalPagesChange={handleTotalPagesChange}
        />
        <Stack
          direction="row"
          spacing={2}
          justifyContent="space-between"
          width="100%"
          px={isMobile ? 1 : 2}
        >
          <Stack>
            <Typography
              variant="caption"
              color="text.disabled"
              sx={{ fontSize: `${0.75 * fontScale}rem` }}
            >
              Page {absoluteCurrentPage} of {absoluteTotalPages}
            </Typography>
          </Stack>
          <Typography
            variant="caption"
            color="text.disabled"
            sx={{ fontSize: `${0.75 * fontScale}rem` }}
          >
            {progressPercent}%
          </Typography>
        </Stack>
      </Stack>
      <PaginationButton
        onClick={handleNext}
        disabled={
          currentPage === totalPages - 1 &&
          currentSection === content.sections.length - 1
        }
        direction="next"
      />
    </Stack>
  );
};

interface PaginationButtonProps {
  onClick: () => void;
  disabled: boolean;
  direction: "prev" | "next";
}

const PaginationButton: React.FC<PaginationButtonProps> = ({
  onClick,
  disabled,
  direction,
}) => {
  const { isMobile } = useMobile();

  const Icon = direction === "prev" ? NavigateBeforeIcon : NavigateNextIcon;

  return (
    <IconButton
      onClick={onClick}
      disabled={disabled}
      size="large"
      disableRipple
      sx={{
        borderTopLeftRadius: direction === "prev" ? "50px" : "10px",
        borderBottomLeftRadius: direction === "prev" ? "50px" : "10px",
        borderTopRightRadius: direction === "prev" ? "10px" : "50px",
        borderBottomRightRadius: direction === "prev" ? "10px" : "50px",
        width: "150px",
        height: "100%",
        "&:hover": {
          backgroundColor: "transparent",
        },
        ...(isMobile && {
          position: "absolute",
          left: direction === "prev" ? 5 : "auto",
          right: direction === "next" ? 5 : "auto",
          zIndex: 1,
        }),
      }}
    >
      {!isMobile && <Icon fontSize="large" />}
    </IconButton>
  );
};
