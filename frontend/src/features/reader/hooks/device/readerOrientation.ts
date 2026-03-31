import { registerPlugin } from "@capacitor/core";

interface ReaderOrientationPlugin {
  lockCurrentReaderOrientation(): Promise<void>;
  restoreReaderOrientation(): Promise<void>;
  unlockReaderOrientation(): Promise<void>;
  restoreAppOrientation(): Promise<void>;
}

const ReaderOrientation = registerPlugin<ReaderOrientationPlugin>(
  "ReaderOrientation",
  {
    web: async () => ({
      lockCurrentReaderOrientation: async () => undefined,
      restoreReaderOrientation: async () => undefined,
      unlockReaderOrientation: async () => undefined,
      restoreAppOrientation: async () => undefined,
    }),
  },
);

export const lockCurrentReaderOrientation = async (): Promise<void> => {
  await ReaderOrientation.lockCurrentReaderOrientation();
};

export const restoreReaderOrientation = async (): Promise<void> => {
  await ReaderOrientation.restoreReaderOrientation();
};

export const unlockReaderOrientation = async (): Promise<void> => {
  await ReaderOrientation.unlockReaderOrientation();
};

export const restoreAppOrientation = async (): Promise<void> => {
  await ReaderOrientation.restoreAppOrientation();
};
