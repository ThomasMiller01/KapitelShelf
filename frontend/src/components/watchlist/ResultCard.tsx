import React from "react";

import bookCover from "../../assets/books/nocover.png";
import { useMobile } from "../../hooks/useMobile";
import type { BookDTO } from "../../lib/api/KapitelShelf.Api";
import { LocationUrl } from "../../utils/LocationUtils";
import { FormatTimeUntil } from "../../utils/TimeUtils";
import ItemCardLayout, {
  MetadataItem,
} from "../layout/ItemCard/ItemCardLayout";

interface ResultCardProps {
  book: BookDTO;
}

export const ResultCard: React.FC<ResultCardProps> = ({ book }) => {
  const { isMobile } = useMobile();

  const timeUntilRelease = FormatTimeUntil(book.releaseDate, false, "calender");
  const isReleased = timeUntilRelease === "-";

  return (
    <ItemCardLayout
      itemVariant="normal"
      small
      raised
      title={book.title}
      link={LocationUrl(book.location!)}
      externalLink
      description={book.description}
      image={book.cover?.filePath}
      fallbackImage={bookCover}
      badge={
        book.seriesNumber !== 0 ? book.seriesNumber?.toString() : undefined
      }
      metadata={[
        <MetadataItem
          sx={{
            fontSize: MetadataFontSize(isMobile),
            height: MetadataFontSize(isMobile),
          }}
          key="pages"
        >
          {book.pageNumber &&
            book.pageNumber !== 0 &&
            `${book.pageNumber} pages`}
        </MetadataItem>,
        <MetadataItem
          sx={{
            fontSize: "0.75rem",
            color: isReleased ? "success.light" : "info.light",
          }}
          key="release-date"
        >
          {isReleased ? "Book is Released!" : `Release ${timeUntilRelease} ...`}
        </MetadataItem>,
      ]}
    />
  );
};

const MetadataFontSize = (isMobile: boolean): string => {
  if (isMobile) {
    // mobile
    return "0.6rem !important";
  }

  // desktop
  return "0.7rem !important";
};
