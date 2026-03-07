import { Box, styled } from "@mui/material";
import { useState, type ReactElement } from "react";

import LoadingCard from "../../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../../components/base/feedback/RequestErrorCard";
import { DRAWER_WIDTH } from "../../../components/base/ResponsiveDrawer";
import { useMobile } from "../../../hooks/useMobile";
import { useReadBook } from "../../../hooks/useReadBook";
import type { BookDTO } from "../../../lib/api/KapitelShelf.Api/api";
import { Content } from "./Content";
import { Sidebar } from "./Sidebar";
import { Toolbar } from "./Toolbar";

const ContentWrapper = styled("div", {
  shouldForwardProp: (prop) => prop !== "open" && prop !== "isMobile",
})<{ open: boolean; isMobile: boolean }>(({ theme, open, isMobile }) => ({
  flexGrow: 1,
  minWidth: 0,
  transition: theme.transitions.create("margin", {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.leavingScreen,
  }),
  marginLeft: isMobile ? 0 : `${DRAWER_WIDTH}px`,
  ...(!open && !isMobile && { marginLeft: 0 }),
  marginTop: isMobile ? theme.spacing(7) : theme.spacing(8),
}));

interface BookDetailsProps {
  book: BookDTO;
}

const BookReader = ({ book }: BookDetailsProps): ReactElement => {
  const { isMobile } = useMobile();
  const { content, isLoading, error } = useReadBook(book);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  if (isLoading) {
    return (
      <LoadingCard
        useLogo
        delayed
        itemName="Book"
        loadingText="Parsing"
        showRandomFacts
      />
    );
  }

  if (error || !content) {
    return (
      <RequestErrorCard itemName="book" actionText="parse" subtitle={error} />
    );
  }

  console.log(content);

  return (
    <Box>
      <Toolbar
        content={content}
        sidebarOpen={sidebarOpen}
        openSidebar={() => setSidebarOpen(true)}
      />
      <Sidebar
        content={content}
        sidebarOpen={sidebarOpen}
        closeSidebar={() => setSidebarOpen(false)}
      />
      <ContentWrapper open={sidebarOpen} isMobile={isMobile}>
        <Content content={content} />
      </ContentWrapper>
    </Box>
  );
};

export default BookReader;
