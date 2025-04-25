import { Box, Chip, Grid, Stack, Typography } from "@mui/material";
import { type ReactElement, useState } from "react";

import bookCover from "../assets/books/nocover.png";
import { useMobile } from "../hooks/useMobile";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";

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
            {book.releaseDate && (
              <Typography variant="body2">
                📅 Release: {new Date(book.releaseDate).toLocaleDateString()}
              </Typography>
            )}
            {book.pageNumber && (
              <Typography variant="body2">
                📄 {book.pageNumber} pages
              </Typography>
            )}
            {book.series?.name && (
              <Typography variant="body2">
                📚 Series: {book.series.name} #{book.seriesNumber}
              </Typography>
            )}
            {book.author && (
              <Typography variant="body2">
                ✍️ Author: {book.author.firstName} {book.author.lastName}
              </Typography>
            )}
          </Stack>

          <Stack direction="row" spacing={1} mb={1} flexWrap="wrap">
            {book.categories &&
              book.categories.map((cat) => (
                <Chip
                  key={cat.id}
                  label={cat.name}
                  color="primary"
                  size="small"
                />
              ))}
          </Stack>

          <Stack direction="row" spacing={1} flexWrap="wrap">
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
