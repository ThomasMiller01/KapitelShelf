import { Stack } from "@mui/material";
import { type ReactElement } from "react";

import bookCover from "../assets/books/nocover.png";
import { useMobile } from "../hooks/useMobile";
import { LocationTypeToString } from "../lib/api/KapitelShelf.Api";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";
import ItemCardLayout, { MetadataItem } from "./layout/ItemCardLayout";

interface BookCardProps {
  book: BookDTO;
  showAuthor?: boolean;
  showMetadata?: boolean;
}

const BookCard = ({
  book,
  showAuthor = false,
  showMetadata = true,
}: BookCardProps): ReactElement => {
  const { isMobile } = useMobile();

  return (
    <ItemCardLayout
      title={book.title ?? ""}
      image={book.cover?.filePath ?? undefined}
      fallbackImage={bookCover}
      badge={
        book.seriesNumber !== 0 ? book.seriesNumber?.toString() : undefined
      }
      metadata={[
        showAuthor ? (
          <MetadataItem key="author">
            {book.author?.firstName} {book.author?.lastName}
          </MetadataItem>
        ) : (
          <></>
        ),
        showMetadata ? (
          <Stack
            key="location-pagenumber"
            direction="row"
            justifyContent="space-between"
            spacing={0}
          >
            <MetadataItem sx={{ fontSize: isMobile ? "0.6rem" : "0.8rem" }}>
              {LocationTypeToString(book.location?.type)}
            </MetadataItem>
            <MetadataItem sx={{ fontSize: isMobile ? "0.6rem" : "0.8rem" }}>
              {book.pageNumber} pages
            </MetadataItem>
          </Stack>
        ) : (
          <></>
        ),
      ]}
    />
  );
};

export default BookCard;
