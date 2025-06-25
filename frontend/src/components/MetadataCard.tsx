import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import CollectionsBookmarkIcon from "@mui/icons-material/CollectionsBookmark";
import DescriptionIcon from "@mui/icons-material/Description";
import PersonIcon from "@mui/icons-material/Person";
import { Box, Chip } from "@mui/material";
import { type ReactElement } from "react";

import bookCover from "../assets/books/nocover.png";
import { useMobile } from "../hooks/useMobile";
import type {
  MetadataDTO,
  MetadataSources,
} from "../lib/api/KapitelShelf.Api/api";
import { MetadataSourceToString } from "../utils/MetadataUtils";
import ItemCardLayout, {
  ItemsMetadata,
  MetadataItem,
} from "./layout/ItemCard/ItemCardLayout";

interface MetadataCardProps {
  metadata: MetadataDTO;
  onClick?: (metadata: MetadataDTO) => void;
  source?: MetadataSources;
}

const MetadataCard = ({
  metadata,
  onClick,
  source,
}: MetadataCardProps): ReactElement => (
  <ItemCardLayout
    itemVariant="detailed"
    title={metadata.title}
    description={metadata.description}
    onClick={onClick ? (): void => onClick(metadata) : undefined}
    image={metadata.coverUrl}
    fallbackImage={bookCover}
    squareBadge={true}
    metadata={[
      <AuthorMetadata key="author" metadata={metadata} />,
      <PagesMetadata key="pages" metadata={metadata} />,
      <ReleaseDateMetadata key="release-date" metadata={metadata} />,
      <SeriesMetadata key="series" metadata={metadata} />,
      <CategoriesMetadata key="categories" metadata={metadata} />,
      <TagsMetadata key="tags" metadata={metadata} />,
      <SourceMetadata key="source" source={source} />,
    ]}
  />
);

interface SourceMetadataProps {
  source?: MetadataSources;
}

const SourceMetadata = ({ source }: SourceMetadataProps): ReactElement => {
  if (source === undefined) {
    return <></>;
  }

  return (
    <Box width="100%" justifyContent="flex-end" display="flex">
      <Chip
        label={MetadataSourceToString[source]}
        size="small"
        color="secondary"
        variant="filled"
      />
    </Box>
  );
};

interface MetadataProps {
  metadata: MetadataDTO;
}

const AuthorMetadata = ({ metadata }: MetadataProps): ReactElement => {
  const { isMobile } = useMobile();

  let author = undefined;
  if (metadata.authors && metadata.authors.length > 0) {
    author = metadata.authors[0];
  }

  if (!author) {
    return <></>;
  }

  return (
    <MetadataItem
      icon={<PersonIcon />}
      metadataVariant="detailed"
      sx={{ fontSize: isMobile ? "0.7rem" : "0.9rem" }}
    >
      {author}
    </MetadataItem>
  );
};

const PagesMetadata = ({ metadata }: MetadataProps): ReactElement => {
  const { isMobile } = useMobile();

  if (!metadata.pages || metadata.pages === 0) {
    return <></>;
  }

  return (
    <MetadataItem
      icon={<DescriptionIcon />}
      metadataVariant="detailed"
      sx={{ fontSize: isMobile ? "0.7rem" : "0.9rem" }}
    >
      {metadata.pages} pages
    </MetadataItem>
  );
};

const ReleaseDateMetadata = ({ metadata }: MetadataProps): ReactElement => {
  const { isMobile } = useMobile();

  if (!metadata.releaseDate) {
    return <></>;
  }

  const releaseDate = new Date(metadata.releaseDate).toLocaleDateString(
    undefined,
    {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
    }
  );

  return (
    <MetadataItem
      icon={<CalendarMonthIcon />}
      metadataVariant="detailed"
      sx={{ fontSize: isMobile ? "0.7rem" : "0.9rem" }}
    >
      {releaseDate}
    </MetadataItem>
  );
};

const SeriesMetadata = ({ metadata }: MetadataProps): ReactElement => {
  const { isMobile } = useMobile();

  if (!metadata.series) {
    return <></>;
  }

  return (
    <MetadataItem
      icon={<CollectionsBookmarkIcon />}
      metadataVariant="detailed"
      sx={{ fontSize: isMobile ? "0.7rem" : "0.9rem" }}
    >
      {metadata.series} #{metadata.volume}
    </MetadataItem>
  );
};

const CategoriesMetadata = ({ metadata }: MetadataProps): ReactElement => {
  if (!metadata.categories || metadata.categories.length === 0) {
    return <></>;
  }

  return (
    <ItemsMetadata
      items={metadata.categories}
      maxItems={3}
      variant="filled"
      color="primary"
    />
  );
};

const TagsMetadata = ({ metadata }: MetadataProps): ReactElement => {
  if (!metadata.tags || metadata.tags.length === 0) {
    return <></>;
  }

  return (
    <ItemsMetadata
      items={metadata.tags}
      maxItems={3}
      variant="outlined"
      color="default"
    />
  );
};

export default MetadataCard;
