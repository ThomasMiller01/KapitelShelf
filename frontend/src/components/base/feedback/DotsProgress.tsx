import { Typography } from "@mui/material";
import type { ReactElement } from "react";
import { useEffect, useState } from "react";

const DEFAULT_DOTS_COUNT = 3;
const DEFAULT_DOTS_DELAY = 1000;

export interface DotsProgressProps {
  initialDots?: number;

  dotsCount?: number;
  dotsDelay?: number;

  small?: boolean;
}

export const DotsProgress = ({
  initialDots = 0,
  dotsCount = DEFAULT_DOTS_COUNT,
  dotsDelay = DEFAULT_DOTS_DELAY,
  small = false,
}: DotsProgressProps): ReactElement => {
  const [dotCount, setDotCount] = useState(initialDots);

  useEffect(() => {
    const interval = setInterval(() => {
      setDotCount((prev) => (prev >= dotsCount ? 1 : prev + 1));
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
      fontSize={small ? "1.2rem" : "1.5rem"}
      letterSpacing={small ? "0.0075em" : "5px"}
    >
      {dotSymbol.repeat(dotCount)}
    </Typography>
  );
};
