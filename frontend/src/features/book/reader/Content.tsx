import { Box, Stack } from "@mui/material";
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
      <Box>{"<"}</Box>
      <ContentSection section={content.sections[currentSection]} />
      <Box>{">"}</Box>
    </Stack>
  );
};
