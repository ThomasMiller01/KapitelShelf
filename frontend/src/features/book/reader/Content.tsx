import NavigateBeforeIcon from "@mui/icons-material/NavigateBefore";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";
import { IconButton, Stack } from "@mui/material";
import { RequestErrorCard } from "../../../components/base/feedback/RequestErrorCard";
import { useMobile } from "../../../hooks/useMobile";
import { BookContent } from "../../../utils/bookReader/BookContent";
import { ContentSection } from "./ContentSection";

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
      direction="row"
      spacing={2}
    >
      <PaginationButton
        onClick={prevSection}
        disabled={currentSection === 0}
        direction="prev"
      />
      <ContentSection section={content.sections[currentSection]} />
      <PaginationButton
        onClick={nextSection}
        disabled={currentSection === content.sections.length - 1}
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
        "&:hover": {
          backgroundColor: "transparent",
        },
        ...(isMobile && {
          transform:
            direction === "prev" ? "translateX(110px)" : "translateX(-110px)",
        }),
      }}
    >
      {!isMobile && <Icon fontSize="large" />}
    </IconButton>
  );
};
