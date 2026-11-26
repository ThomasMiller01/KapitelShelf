import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { Box, styled } from "@mui/material";
import { type ReactElement, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";

import type { BookDTO, ReadingBookDTO } from "../../../lib/api/KapitelShelf.Api/api";
import LoadingCard from "../../../shared/components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../../shared/components/base/feedback/RequestErrorCard";
import { DRAWER_WIDTH } from "../../../shared/components/base/ResponsiveDrawer";
import { useUserProfile } from "../../../shared/hooks/useUserProfile";
import {
  IsMobileApp,
  MOBILE_APP_BOTTOM_INSET,
} from "../../../shared/utils/MobileUtils";
import { useMarkReadingBook } from "../hooks/api/useMarkReadingBook";
import { useReaderOrientation } from "../hooks/device/useReaderOrientation";
import { useReaderCompactLayout } from "../hooks/layout/useReaderCompactLayout";
import { usePersistReadingPosition } from "../hooks/usePersistReadingPosition";
import { useReadBook } from "../hooks/useReadBook";
import { useReaderPosition } from "../hooks/useReaderPosition";
import type { BookContent } from "../utils/BookContentModels";
import { readLocation } from "../utils/readerPosition";
import { Content } from "./Content";
import { Sidebar } from "./Sidebar";
import { Toolbar } from "./Toolbar";

const ContentWrapper = styled("div", {
  shouldForwardProp: (prop) => prop !== "open" && prop !== "isCompactLayout",
})<{ open: boolean; isCompactLayout: boolean }>(
  ({ theme, open, isCompactLayout }) => ({
    display: "flex",
    flexDirection: "column",
    flexGrow: 1,
    minWidth: 0,
    width: "100%",
    height: `calc(100vh - ${IsMobileApp() ? MOBILE_APP_BOTTOM_INSET : "0px"})`,
    transition: theme.transitions.create("margin", {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.leavingScreen,
    }),
    marginLeft: isCompactLayout ? 0 : `-${DRAWER_WIDTH}px`,
    ...(open && !isCompactLayout && { marginLeft: 0 }),
  }),
);

interface BookDetailsProps {
  book: BookDTO;
}

const BookReader = ({ book }: BookDetailsProps): ReactElement => {
  const bookId = book.id!;
  const { content, isLoading, error, retry } = useReadBook(book);
  const [params] = useSearchParams();
  const bookmark = useMarkReadingBook(bookId);
  const { profile } = useUserProfile();
  const navigate = useNavigate();
  useReaderOrientation();

  if (isLoading || (content && bookmark.isFetching && !readLocation(params))) {
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
          onRetry={retry}
          secondAction={() => navigate(`/library/books/${bookId}`)}
          secondActionText="Back to Library"
          secondActionIcon={<ArrowBackIcon />}
        />
      </Box>
    );
  }

  return <ReadingView key={`${bookId}:${profile?.id ?? ""}`} bookId={bookId} content={content} saved={bookmark.data} />;
};

interface ReadingViewProps {
  bookId: string;
  content: BookContent;
  saved?: ReadingBookDTO;
}

const ReadingView = ({ bookId, content, saved }: ReadingViewProps): ReactElement => {
  const { isCompactLayout } = useReaderCompactLayout();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [controlsVisible, setControlsVisible] = useState(false);
  const position = useReaderPosition(content.sections.length, saved);
  usePersistReadingPosition(bookId, position.lastMove);

  return (
    <>
      {controlsVisible && <Toolbar
        content={content}
        isCompactLayout={isCompactLayout}
        sidebarOpen={sidebarOpen}
        openSidebar={() => setSidebarOpen(true)}
      />}
      <Sidebar
        bookId={bookId}
        content={content}
        isCompactLayout={isCompactLayout}
        sidebarOpen={sidebarOpen}
        closeSidebar={() => setSidebarOpen(false)}
        onTocItemSelect={(item) => {
          if (item.sectionIndex !== undefined) {
            position.jump(item.sectionIndex, 0);
          }
        }}
      />
      <ContentWrapper open={sidebarOpen} isCompactLayout={isCompactLayout}>
        <Content
          content={content}
          isCompactLayout={isCompactLayout}
          controlsVisible={controlsVisible}
          onToggleControls={() => setControlsVisible((visible) => !visible)}
          position={position}
          savedPage={saved?.currentPage}
          savedSection={saved?.currentSection}
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
