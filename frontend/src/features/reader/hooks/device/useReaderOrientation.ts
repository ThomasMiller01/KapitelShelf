import { useEffect, useRef } from "react";

import {
  lockCurrentReaderOrientation,
  restoreAppOrientation,
  restoreReaderOrientation,
  unlockReaderOrientation,
} from "./readerOrientation";
import { useReaderColorScheme } from "../../components/ThemeProvider";

export const useReaderOrientation = (): void => {
  const { readerRotationEnabled } = useReaderColorScheme();
  const isInitialMountRef = useRef(true);
  const readerRotationEnabledOnMountRef = useRef(readerRotationEnabled);

  useEffect(() => {
    if (readerRotationEnabledOnMountRef.current) {
      void restoreReaderOrientation();
    }
  }, []);

  useEffect(() => {
    if (isInitialMountRef.current) {
      isInitialMountRef.current = false;
      return;
    }

    if (readerRotationEnabled) {
      void lockCurrentReaderOrientation();
      return;
    }

    void unlockReaderOrientation();
  }, [readerRotationEnabled]);

  useEffect(() => {
    return () => {
      void restoreAppOrientation();
    };
  }, []);
};
