import TipsAndUpdatesIcon from "@mui/icons-material/TipsAndUpdates";
import { Stack, Typography } from "@mui/material";
import type { ReactElement } from "react";
import { useEffect } from "react";

import { useTrivia } from "../hooks/useBookFacts";

const NEXT_TIMEOUT = 10;

const BookFactsCard = (): ReactElement => {
  const { fact, next } = useTrivia();

  useEffect(() => {
    const nextTimer = setTimeout(() => next(), NEXT_TIMEOUT * 1000);

    return (): void => {
      clearTimeout(nextTimer);
    };
  }, [next]);

  return (
    <Stack
      direction="row"
      spacing={1}
      alignItems="start"
      sx={{ textAlign: "center", color: "text.secondary" }}
    >
      <TipsAndUpdatesIcon sx={{ fontSize: "1.2rem" }} />{" "}
      <Typography variant="body2">{fact}</Typography>
    </Stack>
  );
};

export default BookFactsCard;
