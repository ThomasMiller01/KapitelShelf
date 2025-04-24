import { type ReactElement } from "react";

import bookCover from "../assets/books/nocover.png";
import type { SeriesSummaryDTO } from "../lib/api/KapitelShelf.Api/api";
import ItemCardLayout, { MetadataItem } from "./layout/ItemCardLayout";

interface SeriesCardProps {
  serie: SeriesSummaryDTO;
}

const SeriesCard = ({ serie }: SeriesCardProps): ReactElement => {
  const book = serie.lastVolume;
  if (book === undefined) {
    return <></>;
  }

  return (
    <ItemCardLayout
      title={book.title ?? ""}
      image={book.cover?.filePath ?? undefined}
      fallbackImage={bookCover}
      metadata={[
        <MetadataItem>
          {book.author?.firstName} {book.author?.lastName}
        </MetadataItem>,
      ]}
    />
  );
};

export default SeriesCard;
