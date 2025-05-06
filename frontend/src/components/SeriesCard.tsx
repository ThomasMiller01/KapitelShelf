import { type ReactElement } from "react";

import bookCover from "../assets/books/nocover.png";
import { useMobile } from "../hooks/useMobile";
import type { SeriesSummaryDTO } from "../lib/api/KapitelShelf.Api/api";
import { CoverUrl } from "../utils/FileUtils";
import ItemCardLayout, { MetadataItem } from "./layout/ItemCardLayout";

interface SeriesCardProps {
  serie: SeriesSummaryDTO;
}

const SeriesCard = ({ serie }: SeriesCardProps): ReactElement => {
  const { isMobile } = useMobile();

  const book = serie.lastVolume;
  return (
    <ItemCardLayout
      title={serie.name}
      link={`/library/series/${serie.id}`}
      image={CoverUrl(book)}
      fallbackImage={bookCover}
      metadata={[
        <MetadataItem
          key="author"
          sx={{ fontSize: isMobile ? "0.7rem" : "0.9rem" }}
        >
          {book?.author?.firstName} {book?.author?.lastName}
        </MetadataItem>,
      ]}
    />
  );
};

export default SeriesCard;
