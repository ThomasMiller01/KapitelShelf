import { type ReactElement } from "react";
import { useParams } from "react-router-dom";

import { useBookById } from "../../features/book";
import BookReader, {
  ReaderThemeProvider,
  useReaderStatusBar,
} from "../../features/reader";
import LoadingCard from "../../shared/components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../shared/components/base/feedback/RequestErrorCard";

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
