import { Box, Typography } from "@mui/material";
import type { ReactElement } from "react";
import { useEffect, useState } from "react";

import BookFactsCard from "../../BookFactsCard";
import { DotsProgress } from "./DotsProgress";

const DEFAULT_DELAY = 1000;

interface LoadingCardProps {
  useLogo?: boolean;
  itemName?: string;
  delayed?: boolean;
  showRandomFacts?: boolean;
  small?: boolean;
}

const LoadingCard = ({
  useLogo = false,
  itemName,
  delayed = false,
  showRandomFacts = false,
  small = false,
}: LoadingCardProps): ReactElement => {
  const [show, setShow] = useState(false);

  useEffect(() => {
    if (!delayed) {
      setShow(true);
      return;
    }

    const timer = setTimeout(() => setShow(true), DEFAULT_DELAY);
    return (): void => clearTimeout(timer);
  }, [delayed]);

  if (!show) {
    return <></>;
  }

  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        height: "100%",
        py: small ? 0 : 10,
      }}
    >
      {useLogo && <img src="/logo.png" alt="KapitelShelf Logo" width={120} />}
      <Typography
        variant="h5"
        mt={small ? 0 : 4}
        mb={small ? 0 : 3}
        fontSize={small ? "1.2rem !important" : "1.5rem !important"}
        textTransform="uppercase"
      >
        Loading {itemName}
        <DotsProgress small />
      </Typography>
      {showRandomFacts && <BookFactsCard />}
    </Box>
  );
};

export default LoadingCard;
