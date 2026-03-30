import { type ReactElement } from "react";
import { useParams } from "react-router-dom";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import BookReader, { ReaderThemeProvider } from "../../features/reader";
import { useReaderStatusBar } from "../../features/reader/hooks/device/useReaderStatusBar";
import { useBookById } from "../../lib/requests/books/useBookById";

const ReadBookPage = (): ReactElement => {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const { data: book, isLoading, isError, refetch } = useBookById(bookId);

  useReaderStatusBar();

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Book" showRandomFacts />;
  }

  if (isError || book === undefined || book === null) {
    return <RequestErrorCard itemName="book" onRetry={refetch} />;
  }

  return (
    <ReaderThemeProvider>
      <BookReader book={book} />
    </ReaderThemeProvider>
  );
};

export default ReadBookPage;
