import { Stack } from "@mui/material";
import React, { type ReactElement } from "react";

import bookCover from "../assets/books/nocover.png";
import { useMobile } from "../hooks/useMobile";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";
import { CoverUrl } from "../utils/FileUtils";
import { LocationTypeToString } from "../utils/LocationUtils";
import type { ItemCardType } from "./layout/ItemCard/ItemCardLayout";
import ItemCardLayout, { MetadataItem } from "./layout/ItemCard/ItemCardLayout";

interface BookCardProps {
  book: BookDTO;
  showAuthor?: boolean;
  showMetadata?: boolean;
  itemVariant?: ItemCardType;
  onClick?: () => void;
}

const BookCard = ({
  book,
  showAuthor = false,
  showMetadata = true,
  itemVariant = "normal",
  onClick,
}: BookCardProps): ReactElement => {
  const { isMobile } = useMobile();

  return (
    <ItemCardLayout
      itemVariant={itemVariant}
      title={book.title}
      description={book.description}
      link={`/library/books/${book.id}`}
      image={CoverUrl(book)}
      onClick={onClick}
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
          <React.Fragment key="no-author" />
        ),
        showMetadata ? (
          <Stack
            key="location-pagenumber"
            direction="row"
            justifyContent="space-between"
            spacing={0}
            width="100%"
          >
            <MetadataItem sx={{ fontSize: isMobile ? "0.6rem" : "0.8rem" }}>
              {LocationTypeToString[book.location?.type ?? -1]}
            </MetadataItem>
            {book.pageNumber && book.pageNumber !== 0 && (
              <MetadataItem sx={{ fontSize: isMobile ? "0.6rem" : "0.8rem" }}>
                {book.pageNumber} pages
              </MetadataItem>
            )}
          </Stack>
        ) : (
          <React.Fragment key="no-metadata" />
        ),
      ]}
    />
  );
};

export default BookCard;
