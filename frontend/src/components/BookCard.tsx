import { Stack } from "@mui/material";
import React, { type ReactElement } from "react";

import bookCover from "../assets/books/nocover.png";
import { useMobile } from "../hooks/useMobile";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";
import { CoverUrl } from "../utils/FileUtils";
import { LocationTypeToString } from "../utils/LocationTypeUtils";
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
      title={book.title}
      link={`/library/books/${book.id}`}
      image={CoverUrl(book)}
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
          <React.Fragment key="no-author"></React.Fragment>
        ),
        showMetadata ? (
          <Stack
            key="location-pagenumber"
            direction="row"
            justifyContent="space-between"
            spacing={0}
          >
            <MetadataItem sx={{ fontSize: isMobile ? "0.6rem" : "0.8rem" }}>
              {LocationTypeToString[book.location?.type ?? -1]}
            </MetadataItem>
            <MetadataItem sx={{ fontSize: isMobile ? "0.6rem" : "0.8rem" }}>
              {book.pageNumber} pages
            </MetadataItem>
          </Stack>
        ) : (
          <React.Fragment key="no-metadata"></React.Fragment>
        ),
      ]}
    />
  );
};

export default BookCard;
