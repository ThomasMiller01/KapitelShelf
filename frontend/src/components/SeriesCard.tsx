import { type ReactElement } from "react";

import bookCover from "../assets/books/nocover.png";
import { useMobile } from "../hooks/useMobile";
import type { SeriesDTO } from "../lib/api/KapitelShelf.Api/api";
import { CoverUrl } from "../utils/FileUtils";
import type { ItemCardType } from "./layout/ItemCard/ItemCardLayout";
import ItemCardLayout, { MetadataItem } from "./layout/ItemCard/ItemCardLayout";

interface SeriesCardProps {
  series: SeriesDTO;
  itemVariant?: ItemCardType;
  enableLink?: boolean;
  onClick?: () => void;
}

const SeriesCard = ({
  series,
  itemVariant = "normal",
  enableLink = true,
  onClick,
}: SeriesCardProps): ReactElement => {
  const { isMobile } = useMobile();

  const book = series.lastVolume;
  return (
    <ItemCardLayout
      itemVariant={itemVariant}
      title={series.name}
      link={enableLink ? `/library/series/${series.id}` : undefined}
      image={CoverUrl(book)}
      fallbackImage={bookCover}
      onClick={onClick}
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
