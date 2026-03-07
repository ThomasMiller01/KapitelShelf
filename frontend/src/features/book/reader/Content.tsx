import { Box } from "@mui/material";
import { useMobile } from "../../../hooks/useMobile";
import { BookContent } from "../../../utils/bookReader/BookContent";

interface ContentProps {
  content: BookContent;
}

export const Content: React.FC<ContentProps> = ({ content }) => {
  const { isMobile } = useMobile();
  return <Box p={isMobile ? 0.5 : 2}>TODO</Box>;
};
