import TipsAndUpdatesIcon from "@mui/icons-material/TipsAndUpdates";
import { Stack, Typography } from "@mui/material";
import type { ReactElement } from "react";
import { useEffect } from "react";

import { useBookFact } from "../hooks/useBookFacts";

const NEXT_TIMEOUT = 7;

const BookFactsCard = (): ReactElement => {
  const { fact, next } = useBookFact();

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
      <Typography variant="body2" sx={{ textAlign: "left" }}>
        {fact}
      </Typography>
    </Stack>
  );
};

export default BookFactsCard;
