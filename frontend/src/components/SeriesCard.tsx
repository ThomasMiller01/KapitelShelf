import React, { type ReactElement } from "react";

import { Rating } from "@mui/material";
import bookCover from "../assets/books/nocover.png";
import { useMobile } from "../hooks/useMobile";
import type { SeriesDTO } from "../lib/api/KapitelShelf.Api/api";
import { CoverUrl } from "../utils/FileUtils";
import type { ItemCardType } from "./layout/ItemCard/ItemCardLayout";
import ItemCardLayout, {
  ItemsMetadata,
  MetadataItem,
} from "./layout/ItemCard/ItemCardLayout";

interface SeriesCardProps {
  series: SeriesDTO;
  itemVariant?: ItemCardType;
  showMetadata?: boolean;
  linkEnabled?: boolean;
  selected?: boolean;
  onClick?: () => void;
}

const SeriesCard = ({
  series,
  itemVariant = "normal",
  showMetadata = false,
  linkEnabled = true,
  selected,
  onClick,
}: SeriesCardProps): ReactElement => {
  const { isMobile } = useMobile();

  const book = series.lastVolume;
  return (
    <ItemCardLayout
      itemVariant={itemVariant}
      selected={selected}
      title={series.name}
      link={linkEnabled ? `/library/series/${series.id}` : undefined}
      onClick={onClick}
      image={CoverUrl(book)}
      fallbackImage={bookCover}
      badge={(series.totalBooks ?? 0) > 1 ? series.totalBooks?.toString() : ""}
      squareBadge={false}
      metadata={[
        showMetadata ? (
          <>
            <CategoriesMetadata series={series} />
            <TagsMetadata series={series} />
          </>
        ) : (
          <React.Fragment key="no-metadata" />
        ),
        <MetadataItem
          key="author"
          sx={{ fontSize: isMobile ? "0.7rem" : "0.9rem" }}
        >
          {book?.author?.firstName} {book?.author?.lastName}
        </MetadataItem>,
        series.rating ? (
          <Rating
            key="rating"
            value={series.rating / 2}
            max={5}
            precision={0.5}
            readOnly
            size="small"
            sx={{ mt: 0.5 }}
          />
        ) : (
          <></>
        ),
      ]}
    />
  );
};

interface MetadataProps {
  series: SeriesDTO;
}

const CategoriesMetadata = ({ series }: MetadataProps): ReactElement => {
  if (
    !series.lastVolume?.categories ||
    series.lastVolume?.categories.length === 0
  ) {
    return <></>;
  }

  return (
    <ItemsMetadata
      items={series.lastVolume.categories.map((x) => x.name)}
      maxItems={3}
      variant="filled"
      color="primary"
    />
  );
};

const TagsMetadata = ({ series }: MetadataProps): ReactElement => {
  if (!series.lastVolume?.tags || series.lastVolume?.tags.length === 0) {
    return <></>;
  }

  return (
    <ItemsMetadata
      items={series.lastVolume.tags.map((x) => x.name)}
      maxItems={3}
      variant="outlined"
      color="default"
    />
  );
};

export default SeriesCard;
