import { Card, CardContent, CardMedia, Typography } from "@mui/material";
import type { ReactElement } from "react";

import type { BookDTO } from "../lib/api/KapitelShelf.Api/api";

interface BookCardProps {
  book: BookDTO;
}

const BookCard = ({ book }: BookCardProps): ReactElement => (
  <Card sx={{ height: "100%" }}>
    {book.cover && (
      <CardMedia
        component="img"
        height="160"
        image={book.cover.filePath ?? undefined}
        alt={book.title || "Book Cover"}
      />
    )}
    <CardContent>
      <Typography variant="h6">{book.title}</Typography>
      <Typography variant="body2" color="text.secondary">
        {book.author?.firstName} {book.author?.lastName}
      </Typography>
    </CardContent>
  </Card>
);

export default BookCard;
