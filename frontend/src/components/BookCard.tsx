import type { TypographyProps } from "@mui/material";
import {
  Avatar,
  Box,
  Card,
  CardContent,
  CardMedia,
  Stack,
  styled,
  Typography,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import { type ReactElement, useState } from "react";

import bookCover from "../assets/books/nocover.png";
import { LocationTypeToString } from "../lib/api/KapitelShelf.Api";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";

interface MetadataItemProps extends TypographyProps {
  isMobile?: boolean;
}

export const MetadataItem = styled(Typography, {
  shouldForwardProp: (prop) => prop !== "isMobile", // don't forward custom prop
})<MetadataItemProps>(({ theme, isMobile }) => ({
  fontSize: isMobile ? "0.7rem" : "0.9rem",
  color: theme.palette.text.secondary,
  marginTop: "5px",
}));

interface BookCardProps {
  book: BookDTO;
  showAuthor?: boolean;
  showMetadata?: boolean;
}

const BookCard = ({
  book,
  showAuthor = false,
  showMetadata = true,
}: BookCardProps): ReactElement => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("md"));

  const [imageSrc, setImageSrc] = useState(book.cover?.filePath ?? bookCover);

  return (
    <Card sx={{ height: "100%", position: "relative" }}>
      {book.cover && (
        <CardMedia
          component="img"
          height={isMobile ? "200" : "250"}
          image={imageSrc}
          onError={() => setImageSrc(bookCover)}
          alt={book.title || "Book Cover"}
        />
      )}
      {book.seriesNumber !== 0 && (
        <Box position="absolute" top="2px" right="5px">
          <Avatar
            sx={{
              width: 30,
              height: 30,
              backgroundColor: "rgba(0, 0, 0, 0.3)",
              opacity: "0.8",
              fontSize: "0.8rem",
              fontFamily: "Playwrite AU SA",
              fontWeight: "400",
              lineHeight: "2",
              fontFeatureSettings: '"calt"',
            }}
          >
            {book.seriesNumber}.
          </Avatar>
        </Box>
      )}
      <CardContent sx={{ padding: "12px !important" }}>
        <Typography
          variant="h6"
          sx={{
            fontSize: isMobile ? "0.8rem !important" : "1rem !important",
            display: "-webkit-box",
            WebkitLineClamp: 2,
            WebkitBoxOrient: "vertical",
            overflow: "hidden",
            textOverflow: "ellipsis",
            lineHeight: 1.2,
            minHeight: "2.4em",
            wordBreak: "break-all",
          }}
        >
          {book.title}
        </Typography>
        {showAuthor && (
          <MetadataItem>
            {book.author?.firstName} {book.author?.lastName}
          </MetadataItem>
        )}
        {showMetadata && (
          <Stack direction="row" justifyContent="space-between" spacing={0}>
            <MetadataItem sx={{ fontSize: isMobile ? "0.6rem" : "0.8rem" }}>
              {LocationTypeToString(book.location?.type)}
            </MetadataItem>
            <MetadataItem sx={{ fontSize: isMobile ? "0.6rem" : "0.8rem" }}>
              {book.pageNumber} pages
            </MetadataItem>
          </Stack>
        )}
      </CardContent>
    </Card>
  );
};

export default BookCard;
