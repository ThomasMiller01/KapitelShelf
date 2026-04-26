import { Box, styled } from "@mui/material";
import { useState, type ReactElement } from "react";

import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { useNavigate } from "react-router-dom";
import type { BookDTO } from "../../../lib/api/KapitelShelf.Api/api";
import LoadingCard from "../../../shared/components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../../shared/components/base/feedback/RequestErrorCard";
import { DRAWER_WIDTH } from "../../../shared/components/base/ResponsiveDrawer";
import {
  IsMobileApp,
  MOBILE_APP_BOTTOM_INSET,
} from "../../../shared/utils/MobileUtils";
import { useMarkReadingBook } from "../hooks/api/useMarkReadingBook";
import { useReaderOrientation } from "../hooks/device/useReaderOrientation";
import { useReaderCompactLayout } from "../hooks/layout/useReaderCompactLayout";
import { useReadBook } from "../hooks/useReadBook";
import { useReadBookPagination } from "../hooks/useReadBookPagination";
import { Content } from "./Content";
import { Sidebar } from "./Sidebar";
import { Toolbar } from "./Toolbar";

const ContentWrapper = styled("div", {
  shouldForwardProp: (prop) => prop !== "open" && prop !== "isCompactLayout",
})<{ open: boolean; isCompactLayout: boolean }>(
  ({ theme, open, isCompactLayout }) => ({
    flexGrow: 1,
    minWidth: 0,
    width: "100%",
    height: `calc(100vh - ${
      isCompactLayout ? theme.spacing(7) : theme.spacing(8)
    } - ${IsMobileApp() ? MOBILE_APP_BOTTOM_INSET : "0px"})`,
    transition: theme.transitions.create("margin", {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.leavingScreen,
    }),
    marginLeft: isCompactLayout ? 0 : `-${DRAWER_WIDTH}px`,
    ...(open && !isCompactLayout && { marginLeft: 0 }),
    marginTop: isCompactLayout ? theme.spacing(7) : theme.spacing(8),
  }),
);

interface BookDetailsProps {
  book: BookDTO;
}

const BookReader = ({ book }: BookDetailsProps): ReactElement => {
  const { isCompactLayout } = useReaderCompactLayout();
  const { content, isLoading, error } = useReadBook(book);
  useReaderOrientation();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { section, page, nextSection, prevSection, setSection, setPage } =
    useReadBookPagination();
  const navigate = useNavigate();
  useMarkReadingBook(book.id);

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
        isCompactLayout={isCompactLayout}
        sidebarOpen={sidebarOpen}
        openSidebar={() => setSidebarOpen(true)}
      />
      <Sidebar
        bookId={book.id}
        content={content}
        isCompactLayout={isCompactLayout}
        sidebarOpen={sidebarOpen}
        closeSidebar={() => setSidebarOpen(false)}
        onTocItemSelect={(item) => setSection(item.sectionIndex ?? 0)}
      />
      <ContentWrapper open={sidebarOpen} isCompactLayout={isCompactLayout}>
        <Content
          content={content}
          isCompactLayout={isCompactLayout}
          currentSection={section}
          currentPage={page}
          setCurrentPage={setPage}
          nextSection={nextSection}
          prevSection={prevSection}
        />
      </ContentWrapper>

      {/* Bottom padding for mobile */}
      {IsMobileApp() && (
        <Box
          height={MOBILE_APP_BOTTOM_INSET}
          width="100%"
          bgcolor="background.paper"
          position="absolute"
          bottom={0}
        />
      )}
    </>
  );
};

export default BookReader;
