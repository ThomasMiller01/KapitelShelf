import { type ReactElement } from "react";

import bookCover from "../assets/books/nocover.png";
import { useMobile } from "../hooks/useMobile";
import type { SeriesDTO } from "../lib/api/KapitelShelf.Api/api";
import { CoverUrl } from "../utils/FileUtils";
import ItemCardLayout, { MetadataItem } from "./layout/ItemCardLayout";

interface SeriesCardProps {
  series: SeriesDTO;
}

const SeriesCard = ({ series }: SeriesCardProps): ReactElement => {
  const { isMobile } = useMobile();

  const book = series.lastVolume;
  return (
    <ItemCardLayout
      title={series.name}
      link={`/library/series/${series.id}`}
      image={CoverUrl(book)}
      fallbackImage={bookCover}
      badge={series.totalBooks ? series.totalBooks?.toString() : ""}
      squareBadge={false}
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
