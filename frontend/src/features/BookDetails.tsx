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
interface MetadataItemProps {
  icon: ReactNode;
  children: ReactNode;
}

const MetadataItem = ({ icon, children }: MetadataItemProps): ReactElement => (
  <Stack direction="row" spacing={0.8} alignItems="start">
    {icon && (
      <Box sx={{ "& > *": { fontSize: "1.2rem !important" } }}>{icon}</Box>
    )}
    <Typography variant="body2">{children}</Typography>
  </Stack>
);

interface BookDetailsProps {
  book: BookDTO;
}

const BookDetails = ({ book }: BookDetailsProps): ReactElement => {
  const { isMobile } = useMobile();
  const [imageSrc, setImageSrc] = useState(book.cover?.filePath ?? bookCover);

  return (
    <Box p={3}>
      <Grid container spacing={4}>
        <Grid size={{ xs: 12, md: 3 }}>
          <img
            src={imageSrc ?? undefined}
            onError={() => setImageSrc(bookCover)}
            alt={book.title ?? "Book Cover"}
            style={{
              width: "100%",
              maxWidth: 320,
              borderRadius: 2,
              boxShadow: "3",
              marginLeft: isMobile ? "auto" : 0,
              marginRight: isMobile ? "auto" : 0,
            }}
          />
        </Grid>

        <Grid size={{ xs: 12, md: 8 }} mt="20px">
          <Typography variant="h4" gutterBottom>
            {book.title}
          </Typography>

          <Typography variant="body1" color="text.secondary" paragraph>
            {book.description}
          </Typography>

          <Stack direction="row" spacing={3} flexWrap="wrap" mb={2}>
            {book.pageNumber && (
              <MetadataItem icon={<DescriptionIcon />}>
                {book.pageNumber} pages
              </MetadataItem>
            )}
            {book.author && (
              <MetadataItem icon={<PersonIcon />}>
                {book.author.firstName} {book.author.lastName}
              </MetadataItem>
            )}
            {book.releaseDate && (
              <MetadataItem icon={<CalendarMonthIcon />}>
                {new Date(book.releaseDate).toLocaleDateString()}
              </MetadataItem>
            )}
            <MetadataItem icon={<CollectionsBookmarkIcon />}>
              {book.series?.name} #{book.seriesNumber}
            </MetadataItem>
          </Stack>
          <Stack direction="row" spacing={1} mb={1.5} flexWrap="wrap">
            <CategoryIcon sx={{ mr: "5px !important" }} />
            {book.categories &&
              book.categories.map((category) => (
                <Chip
                  key={category.id}
                  label={category.name}
                  color="primary"
                  size="small"
                />
              ))}
          </Stack>
          <Stack direction="row" spacing={1} flexWrap="wrap">
            <LocalOfferIcon sx={{ mr: "5px !important" }} />
            {book.tags &&
              book.tags.map((tag) => (
                <Chip
                  key={tag.id}
                  label={tag.name}
                  variant="outlined"
                  size="small"
                />
              ))}
          </Stack>
        </Grid>
      </Grid>
    </Box>
  );
};

export default BookDetails;
