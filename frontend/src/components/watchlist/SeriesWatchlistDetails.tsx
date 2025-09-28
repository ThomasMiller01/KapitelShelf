import LaunchIcon from "@mui/icons-material/Launch";
import { Box, Grid, Stack, Typography } from "@mui/material";
import React from "react";
import { Link } from "react-router-dom";

import bookCover from "../../assets/books/nocover.png";
import { useMobile } from "../../hooks/useMobile";
import type { SeriesWatchlistDTO } from "../../lib/api/KapitelShelf.Api";
import { LocationUrl } from "../../utils/LocationUtils";
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
    <Box width="fit-content">
      <Link
        to={`/library/series/${watchlist.series?.id}`}
        style={{
          textDecoration: "none",
          width: "fit-content",
          display: "inline-block",
        }}
      >
        <Stack
          direction="row"
          spacing={1}
          alignItems="center"
          mb={1}
          width="fit-content"
        >
          <Typography variant="h6" gutterBottom color="text.primary">
            {watchlist.series?.name}
          </Typography>
          <LaunchIcon sx={{ fontSize: "1.2rem", color: "text.primary" }} />
        </Stack>
      </Link>

      <Grid container spacing={2}>
        {(watchlist.items ?? []).map((book) => (
          <Grid key={book.id}>
            <ItemCardLayout
              itemVariant="normal"
              small
              raised
              title={book.title}
              link={LocationUrl(book.location!)}
              externalLink={true}
              // onClick={() => alert("test")}
              description={book.description}
              image={book.cover?.filePath}
              fallbackImage={bookCover}
              badge={
                book.seriesNumber !== 0
                  ? book.seriesNumber?.toString()
                  : undefined
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
          </Grid>
        ))}
      </Grid>

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
