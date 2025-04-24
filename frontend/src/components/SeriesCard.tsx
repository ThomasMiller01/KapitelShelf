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
      title={serie.name}
      link={`/library/${serie.id}`}
      image={book.cover?.filePath}
      fallbackImage={bookCover}
      metadata={[
        <MetadataItem key="author">
          {book.author?.firstName} {book.author?.lastName}
        </MetadataItem>,
      ]}
    />
  );
};

export default SeriesCard;
