import {
  Card,
  CardContent,
  CardMedia,
  Typography,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import type { ReactElement } from "react";

import bookCover from "../assets/books/nocover.png";
import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";

interface BookCardProps {
  book: BookDTO;
}

const BookCard = ({ book }: BookCardProps): ReactElement => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("md"));

  return (
    <Card sx={{ height: "100%" }}>
      {book.cover && (
        <CardMedia
          component="img"
          height={isMobile ? "380" : "260"}
          // image={book.cover.filePath ?? bookCover}
          image={bookCover}
          alt={book.title || "Book Cover"}
        />
      )}
      <CardContent sx={{ padding: "12px !important" }}>
        <Typography
          variant="h6"
          sx={{
            fontSize: "1rem !important",
            display: "-webkit-box",
            WebkitLineClamp: 2,
            WebkitBoxOrient: "vertical",
            overflow: "hidden",
            textOverflow: "ellipsis",
            minHeight: "3em", // force space for 2 lines (approx)
            wordBreak: "break-all",
          }}
        >
          {book.title}
        </Typography>
        <Typography variant="body2" color="text.secondary" mt="10px">
          {book.author?.firstName} {book.author?.lastName}
        </Typography>
      </CardContent>
    </Card>
  );
};

export default BookCard;
