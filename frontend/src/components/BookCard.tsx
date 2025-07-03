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
  small?: boolean;
}

const BookCard = ({
  book,
  showAuthor = false,
  showMetadata = true,
  itemVariant = "normal",
  onClick,
  small = false,
}: BookCardProps): ReactElement => {
  const { isMobile } = useMobile();

  return (
    <ItemCardLayout
      itemVariant={itemVariant}
      small={small}
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
            <MetadataItem sx={{ fontSize: MetadataFontSize(isMobile, small) }}>
              {LocationTypeToString[book.location?.type ?? -1]}
            </MetadataItem>
            {book.pageNumber && book.pageNumber !== 0 && (
              <MetadataItem
                sx={{ fontSize: MetadataFontSize(isMobile, small) }}
              >
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

const MetadataFontSize = (isMobile: boolean, small: boolean): string => {
  if (isMobile) {
    // mobile and normal or small
    return "0.6rem !important";
  }

  if (small) {
    // desktop and small
    return "0.7rem !important";
  }

  // desktop and normal
  return "0.8rem !important";
};

export default BookCard;
