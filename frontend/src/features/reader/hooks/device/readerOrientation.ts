import { registerPlugin } from "@capacitor/core";

interface ReaderOrientationPlugin {
  setReaderOrientationLocked(options: { locked: boolean }): Promise<void>;
  restoreAppOrientation(): Promise<void>;
}

const ReaderOrientation = registerPlugin<ReaderOrientationPlugin>(
  "ReaderOrientation",
  {
    web: async () => ({
      setReaderOrientationLocked: async () => undefined,
      restoreAppOrientation: async () => undefined,
    }),
  },
);

export const setReaderOrientationLocked = async (
  locked: boolean,
): Promise<void> => {
  await ReaderOrientation.setReaderOrientationLocked({ locked });
};

export const restoreAppOrientation = async (): Promise<void> => {
  await ReaderOrientation.restoreAppOrientation();
};
