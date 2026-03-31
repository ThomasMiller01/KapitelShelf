import { useMediaQuery } from "@mui/material";

import { useMobile } from "../../../../hooks/useMobile";

interface ReaderCompactLayoutResult {
  isCompactLayout: boolean;
}

export const useReaderCompactLayout = (): ReaderCompactLayoutResult => {
  const { isMobile } = useMobile();
  const isLandscapePhone = useMediaQuery(
    "(orientation: landscape) and (pointer: coarse) and (max-height: 500px)",
  );

  return {
    isCompactLayout: isMobile || isLandscapePhone,
  };
};
