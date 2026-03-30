import { useEffect } from "react";

import {
  restoreAppOrientation,
  setReaderOrientationLocked,
} from "./readerOrientation";
import { useReaderColorScheme } from "../../components/ThemeProvider";

export const useReaderOrientation = (): void => {
  const { readerRotationEnabled } = useReaderColorScheme();

  useEffect(() => {
    void setReaderOrientationLocked(readerRotationEnabled);
  }, [readerRotationEnabled]);

  useEffect(() => {
    return () => {
      void restoreAppOrientation();
    };
  }, []);
};
