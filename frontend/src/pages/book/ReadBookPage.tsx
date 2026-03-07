import { type ReactElement } from "react";
import { useParams } from "react-router-dom";

import LoadingCard from "../../components/base/feedback/LoadingCard";
import { RequestErrorCard } from "../../components/base/feedback/RequestErrorCard";
import BookReader from "../../features/book/reader/BookReader";
import { useBookById } from "../../lib/requests/books/useBookById";

const ReadBookPage = (): ReactElement => {
  const { bookId } = useParams<{
    bookId: string;
  }>();

  const { data: book, isLoading, isError, refetch } = useBookById(bookId);

  if (isLoading) {
    return <LoadingCard useLogo delayed itemName="Book" showRandomFacts />;
  }

  if (isError || book === undefined || book === null) {
    return <RequestErrorCard itemName="book" onRetry={refetch} />;
  }

  return <BookReader book={book} />;
};

export default ReadBookPage;
