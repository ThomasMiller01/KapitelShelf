import NavigateBeforeIcon from "@mui/icons-material/NavigateBefore";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";
import { IconButton, Stack, Typography } from "@mui/material";
import React, { useCallback, useEffect, useRef, useState } from "react";
import { RequestErrorCard } from "../../../components/base/feedback/RequestErrorCard";
import { useMobile } from "../../../hooks/useMobile";
import { BookContent } from "../../../utils/bookReader/BookContent";
import { ContentSection } from "./ContentSection";
import { useBookPageProgress } from "./useBookPageProgress";

interface ContentProps {
  content: BookContent;
  currentSection: number;
  nextSection: () => void;
  prevSection: () => void;
}

export const Content: React.FC<ContentProps> = ({
  content,
  currentSection,
  nextSection,
  prevSection,
}) => {
  const { isMobile } = useMobile();
  const [currentPage, setCurrentPage] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const navigatedBackRef = useRef(false);

  useEffect(() => {
    if (navigatedBackRef.current) {
      navigatedBackRef.current = false;
      return; // handleTotalPagesChange already set the correct last page
    }

    setCurrentPage(0);
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
      }
    },
    [onPageProgressChange],
  );

  const handleNext = () => {
    if (currentPage < totalPages - 1) {
      setCurrentPage((p) => p + 1);
    } else {
      nextSection();
    }
  };

  const handlePrev = () => {
    if (currentPage > 0) {
      setCurrentPage((p) => p - 1);
    } else {
      navigatedBackRef.current = true;
      prevSection();
    }
  };

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
      py={isMobile ? 2 : 4}
      height="100%"
      maxWidth="100%"
      justifyContent="center"
      alignItems="center"
      direction="row"
      spacing={{ sm: 0, md: 2 }}
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
        <ContentSection
          section={content.sections[currentSection]}
          currentPage={currentPage}
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
            <Typography variant="caption" color="text.disabled">
              Page {absoluteCurrentPage} of {absoluteTotalPages}
            </Typography>
          </Stack>
          <Typography variant="caption" color="text.disabled">
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
      sx={{
        borderTopLeftRadius: direction === "prev" ? "50px" : "10px",
        borderBottomLeftRadius: direction === "prev" ? "50px" : "10px",
        borderTopRightRadius: direction === "prev" ? "10px" : "50px",
        borderBottomRightRadius: direction === "prev" ? "10px" : "50px",
        width: "100px",
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
