import LaunchIcon from "@mui/icons-material/Launch";
import { Box, Stack, Typography } from "@mui/material";
import React from "react";
import { Link } from "react-router-dom";

import bookCover from "../../assets/books/nocover.png";
import { useMobile } from "../../hooks/useMobile";
import type { SeriesWatchlistDTO } from "../../lib/api/KapitelShelf.Api";
import ItemCardLayout, {
  MetadataItem,
} from "../layout/ItemCard/ItemCardLayout";

interface SeriesWatchlistDetailsProps {
  watchlist: SeriesWatchlistDTO;
}

export const SeriesWatchlistDetails: React.FC<SeriesWatchlistDetailsProps> = ({
  watchlist,
}) => {
  const { isMobile } = useMobile();
  return (
    <Box>
      <Link
        to={`/library/series/${watchlist.series?.id}`}
        style={{ textDecoration: "none" }}
      >
        <Stack direction="row" spacing={1} alignItems="center" mb={1}>
          <Typography variant="h6" gutterBottom color="text.primary">
            {watchlist.series?.name}
          </Typography>
          <LaunchIcon sx={{ fontSize: "1.2rem", color: "text.primary" }} />
        </Stack>
      </Link>

      {(watchlist.items ?? []).map((book) => (
        <ItemCardLayout
          itemVariant="normal"
          small
          raised
          title={book.title}
          link={"TODO: save link/asin in db"}
          description={book.description}
          image={book.cover?.filePath}
          fallbackImage={bookCover}
          badge={
            book.seriesNumber !== 0 ? book.seriesNumber?.toString() : undefined
          }
          metadata={[
            book.pageNumber && book.pageNumber !== 0 ? (
              <MetadataItem sx={{ fontSize: MetadataFontSize(isMobile) }}>
                {book.pageNumber} pages
              </MetadataItem>
            ) : (
              <React.Fragment key="no-metadata" />
            ),
          ]}
        />
      ))}

      {(watchlist.items?.length ?? []) === 0 && (
        <Typography variant="body2" color="text.secondary" mt={-0.5}>
          The next volume has not been announced yet.
        </Typography>
      )}
    </Box>
  );
};

const MetadataFontSize = (isMobile: boolean): string => {
  if (isMobile) {
    // mobile
    return "0.6rem !important";
  }

  // desktop
  return "0.7rem !important";
};
