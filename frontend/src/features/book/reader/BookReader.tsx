import { Box, styled } from "@mui/material";
import { useState, type ReactElement } from "react";

import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { useNavigate } from "react-router-dom";
import LoadingCard from "../../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../../components/base/feedback/RequestErrorCard";
import { DRAWER_WIDTH } from "../../../components/base/ResponsiveDrawer";
import { useMobile } from "../../../hooks/useMobile";
import { useReadBook } from "../../../hooks/useReadBook";
import { useReadBookPagination } from "../../../hooks/useReadBookPagination";
import type { BookDTO } from "../../../lib/api/KapitelShelf.Api/api";
import { Content } from "./Content";
import { Sidebar } from "./Sidebar";
import { Toolbar } from "./Toolbar";

const ContentWrapper = styled("div", {
  shouldForwardProp: (prop) => prop !== "open" && prop !== "isMobile",
})<{ open: boolean; isMobile: boolean }>(({ theme, open, isMobile }) => ({
  flexGrow: 1,
  minWidth: 0,
  width: "100%",
  height: `calc(100vh - ${isMobile ? theme.spacing(7) : theme.spacing(8)})`,
  transition: theme.transitions.create("margin", {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.leavingScreen,
  }),
  marginLeft: isMobile ? 0 : `-${DRAWER_WIDTH}px`,
  ...(open && !isMobile && { marginLeft: 0 }),
  marginTop: isMobile ? theme.spacing(7) : theme.spacing(8),
}));

interface BookDetailsProps {
  book: BookDTO;
}

const BookReader = ({ book }: BookDetailsProps): ReactElement => {
  const { isMobile } = useMobile();
  const { content, isLoading, error } = useReadBook(book);
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const {
    section,
    next: nextSection,
    prev: prevSection,
    set: setSection,
  } = useReadBookPagination();
  const navigate = useNavigate();

  if (isLoading) {
    return (
      <Box width="100%" justifyContent="center">
        <LoadingCard
          useLogo
          delayed
          itemName="Book"
          loadingText="Parsing"
          showRandomFacts
        />
      </Box>
    );
  }

  if (error || !content) {
    return (
      <Box width="100%" justifyContent="center">
        <RequestErrorCard
          itemName="book"
          actionText="parse"
          subtitle={error}
          secondAction={() => navigate(`/library/books/${book.id}`)}
          secondActionText="Back to Library"
          secondActionIcon={<ArrowBackIcon />}
        />
      </Box>
    );
  }

  return (
    <>
      <Toolbar
        content={content}
        sidebarOpen={sidebarOpen}
        openSidebar={() => setSidebarOpen(true)}
      />
      <Sidebar
        bookId={book.id}
        content={content}
        sidebarOpen={sidebarOpen}
        closeSidebar={() => setSidebarOpen(false)}
        onTocItemSelect={(item) => setSection(item.sectionIndex ?? 0)}
      />
      <ContentWrapper open={sidebarOpen} isMobile={isMobile}>
        <Content
          content={content}
          currentSection={section}
          nextSection={nextSection}
          prevSection={prevSection}
        />
      </ContentWrapper>
    </>
  );
};

export default BookReader;
