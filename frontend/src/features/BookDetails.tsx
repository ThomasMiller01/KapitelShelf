import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import CategoryIcon from "@mui/icons-material/Category";
import CollectionsBookmarkIcon from "@mui/icons-material/CollectionsBookmark";
import DescriptionIcon from "@mui/icons-material/Description";
import LocalOfferIcon from "@mui/icons-material/LocalOffer";
import PersonIcon from "@mui/icons-material/Person";
import { Box, Chip, Grid, Stack, Typography } from "@mui/material";
import type { ReactNode } from "react";
import { type ReactElement, useState } from "react";

import bookCover from "../assets/books/nocover.png";
import { useMobile } from "../hooks/useMobile";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";
import LocationDetails from "./LocationDetails";

interface MetadataItemProps {
  icon: ReactNode;
  children: ReactNode;
}

const MetadataGridItem = ({
  icon,
  children,
}: MetadataItemProps): ReactElement => (
  <Grid>
    <Stack direction="row" spacing={0.8} alignItems="start">
      {icon && (
        <Box sx={{ "& > *": { fontSize: "1.2rem !important" } }}>{icon}</Box>
      )}
      <Typography variant="body2">{children}</Typography>
    </Stack>
  </Grid>
);

interface BookDetailsProps {
  book: BookDTO;
}

const BookDetails = ({ book }: BookDetailsProps): ReactElement => {
  const { isMobile } = useMobile();

  const [imageSrc, setImageSrc] = useState(
    book.cover?.filePath ? `/data/${book.cover?.filePath}` : bookCover
  );

  return (
    <Box p={3}>
      <Grid container spacing={{ xs: 1, md: 4 }} columns={11}>
        <Grid size={{ xs: 0, md: 0.5 }}></Grid>

        <Grid size={{ xs: 11, md: 2.5 }}>
          <Stack>
            <img
              src={imageSrc ?? undefined}
              onError={() => setImageSrc(bookCover)}
              alt={book.title ?? "Book Cover"}
              style={{
                width: "100%",
                borderRadius: 2,
                boxShadow: "3",
                marginLeft: isMobile ? "auto" : 0,
                marginRight: isMobile ? "auto" : 0,
              }}
            />

            {book.location && <LocationDetails location={book.location} />}
          </Stack>
        </Grid>

        <Grid size={{ xs: 11, md: 7.5 }} mt="20px">
          <Typography variant="h5" gutterBottom>
            {book.title}
          </Typography>

          <Typography variant="body1" color="text.secondary" mb="15px">
            {book.description}
          </Typography>

          <Grid container rowSpacing={1} columnSpacing={2.5} mb={2}>
            {book.pageNumber !== null && (
              <MetadataGridItem icon={<DescriptionIcon />}>
                {book.pageNumber} pages
              </MetadataGridItem>
            )}
            {book.author && (
              <MetadataGridItem icon={<PersonIcon />}>
                {book.author.firstName} {book.author.lastName}
              </MetadataGridItem>
            )}
            {book.releaseDate && (
              <MetadataGridItem icon={<CalendarMonthIcon />}>
                {new Date(book.releaseDate).toLocaleDateString()}
              </MetadataGridItem>
            )}
            <MetadataGridItem icon={<CollectionsBookmarkIcon />}>
              {book.series?.name} #{book.seriesNumber}
            </MetadataGridItem>
          </Grid>

          <Stack direction="row" spacing={1} mb="5px">
            <CategoryIcon sx={{ mr: "5px !important" }} />
            <Stack
              direction="row"
              spacing={1}
              mb={1.5}
              flexWrap="wrap"
              alignItems="center"
            >
              {book.categories &&
                book.categories.map((category) => (
                  <Chip
                    key={category.id}
                    label={category.name}
                    color="primary"
                    size="small"
                    sx={{ my: "4px !important" }}
                  />
                ))}
            </Stack>
            {book.categories?.length === 0 && (
              <Typography variant="subtitle1">No Categories</Typography>
            )}
          </Stack>

          <Stack direction="row" spacing={1}>
            <LocalOfferIcon sx={{ mr: "5px !important" }} />
            <Stack
              direction="row"
              spacing={1}
              flexWrap="wrap"
              alignItems="center"
            >
              {book.tags &&
                book.tags.map((tag) => (
                  <Chip
                    key={tag.id}
                    label={tag.name}
                    variant="outlined"
                    size="small"
                    sx={{ my: "4px !important" }}
                  />
                ))}
            </Stack>
            {book.tags?.length === 0 && (
              <Typography variant="subtitle1">No Tags</Typography>
            )}
          </Stack>
        </Grid>

        <Grid size={{ xs: 0, md: 0.5 }}></Grid>
      </Grid>
    </Box>
  );
};

export default BookDetails;
