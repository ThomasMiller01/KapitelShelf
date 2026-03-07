import { Box } from "@mui/material";
import { useState, type ReactElement } from "react";

import LoadingCard from "../../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../../components/base/feedback/RequestErrorCard";
import { ResponsiveDrawer } from "../../../components/base/ResponsiveDrawer";
import { useReadBook } from "../../../hooks/useReadBook";
import type { BookDTO } from "../../../lib/api/KapitelShelf.Api/api";
import TableOfContents from "./TableOfContents";
import { Toolbar } from "./Toolbar";

interface BookDetailsProps {
  book: BookDTO;
}

const BookReader = ({ book }: BookDetailsProps): ReactElement => {
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
        toggleSidebarOpen={() => setSidebarOpen(true)}
      />
      <ResponsiveDrawer
        open={sidebarOpen}
        onClose={() => setSidebarOpen(false)}
      >
        <TableOfContents items={content?.navigation.tableOfContents ?? []} />
      </ResponsiveDrawer>
    </Box>
  );
};

export default BookReader;
