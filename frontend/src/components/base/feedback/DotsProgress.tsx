import { Typography } from "@mui/material";
import type { ReactElement } from "react";
import { useEffect, useState } from "react";

const DEFAULT_DOTS_COUNT = 3;
const DEFAULT_DOTS_DELAY = 1200;

export interface DotsProgressProps {
  dotsCount?: number;
  dotsDelay?: number;

  small?: boolean;
}

export const DotsProgress = ({
  dotsCount = DEFAULT_DOTS_COUNT,
  dotsDelay = DEFAULT_DOTS_DELAY,
  small = false,
}: DotsProgressProps): ReactElement => {
  const [dotCount, setDotCount] = useState(0);

  useEffect(() => {
    const interval = setInterval(() => {
      setDotCount((prev) => (prev + 1) % (dotsCount + 1));
    }, dotsDelay);

    return (): void => clearInterval(interval);
  }, [dotsCount, dotsDelay]);

  const dotSymbol = small ? "." : "•";

  return (
    <Typography
      variant="caption"
      color="inherit"
      mt={1}
      display="inline"
      fontSize="1.25rem"
      letterSpacing={small ? "0.0075em" : "5px"}
    >
      {dotSymbol.repeat(dotCount)}
    </Typography>
  );
};
