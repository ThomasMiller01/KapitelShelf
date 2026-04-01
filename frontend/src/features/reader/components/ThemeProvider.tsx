import "../styles/ReaderFonts";

import { ThemeProvider } from "@mui/material";
import {
  createContext,
  type ReactNode,
  useCallback,
  useContext,
  useState,
} from "react";
import { createReaderTheme, ReaderColorScheme } from "../../../styles/Theme";

const COLOR_SCHEME_STORAGE_KEY = "kapitelshelf.reader.colorscheme";
const FONT_SCALE_STORAGE_KEY = "kapitelshelf.reader.fontscale";
const CONTENT_FONT_STORAGE_KEY = "kapitelshelf.reader.contentfont";
const READER_ROTATION_STORAGE_KEY = "kapitelshelf.reader.rotation";
export const READER_FONT_SCALE_OPTIONS = [
  0.8, 0.9, 1, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6,
];
const DEFAULT_FONT_SCALE = 1;

export type ReaderContentFont =
  | "literata"
  | "inter"
  | "ebGaramond"
  | "comicNeue";

const DEFAULT_CONTENT_FONT: ReaderContentFont = "literata";

export interface ReaderContentFontOption {
  value: ReaderContentFont;
  label: string;
  family: string;
}

export const READER_CONTENT_FONT_OPTIONS: ReaderContentFontOption[] = [
  { value: "literata", label: "Literata", family: '"Literata", serif' },
  { value: "inter", label: "Inter", family: '"Inter", sans-serif' },
  { value: "ebGaramond", label: "EB Garamond", family: '"EB Garamond", serif' },
  { value: "comicNeue", label: "Comic Neue", family: '"Comic Neue", cursive' },
];

const READER_CONTENT_FONT_FAMILIES: Record<ReaderContentFont, string> =
  READER_CONTENT_FONT_OPTIONS.reduce(
    (acc, option) => ({ ...acc, [option.value]: option.family }),
    {} as Record<ReaderContentFont, string>,
  );

interface ReaderColorSchemeContextValue {
  colorScheme: ReaderColorScheme;
  setColorScheme: (scheme: ReaderColorScheme) => void;
  fontScale: number;
  setFontScale: (scale: number) => void;
  contentFont: ReaderContentFont;
  contentFontFamily: string;
  setContentFont: (font: ReaderContentFont) => void;
  readerRotationEnabled: boolean;
  setReaderRotationEnabled: (enabled: boolean) => void;
}

const ReaderColorSchemeContext = createContext<ReaderColorSchemeContextValue>({
  colorScheme: "light",
  setColorScheme: () => undefined,
  fontScale: DEFAULT_FONT_SCALE,
  setFontScale: () => undefined,
  contentFont: DEFAULT_CONTENT_FONT,
  contentFontFamily: READER_CONTENT_FONT_FAMILIES[DEFAULT_CONTENT_FONT],
  setContentFont: () => undefined,
  readerRotationEnabled: false,
  setReaderRotationEnabled: () => undefined,
});

export const useReaderColorScheme = (): ReaderColorSchemeContextValue =>
  useContext(ReaderColorSchemeContext);

export const themeForScheme = {
  light: createReaderTheme("light"),
  dark: createReaderTheme("dark"),
  sepia: createReaderTheme("sepia"),
};

interface ReaderThemeProviderProps {
  children: ReactNode;
}

export const ReaderThemeProvider = ({
  children,
}: ReaderThemeProviderProps): ReactNode => {
  const [colorScheme, setColorSchemeState] = useState<ReaderColorScheme>(
    () =>
      (localStorage.getItem(COLOR_SCHEME_STORAGE_KEY) as ReaderColorScheme) ??
      "light",
  );
  const [fontScale, setFontScaleState] = useState<number>(() => {
    const value = Number(localStorage.getItem(FONT_SCALE_STORAGE_KEY));
    return READER_FONT_SCALE_OPTIONS.includes(value)
      ? value
      : DEFAULT_FONT_SCALE;
  });
  const [contentFont, setContentFontState] = useState<ReaderContentFont>(() => {
    const stored = localStorage.getItem(CONTENT_FONT_STORAGE_KEY);
    const isValid = READER_CONTENT_FONT_OPTIONS.some(
      (option) => option.value === stored,
    );
    return isValid ? (stored as ReaderContentFont) : DEFAULT_CONTENT_FONT;
  });
  const [readerRotationEnabled, setReaderRotationEnabledState] =
    useState<boolean>(
      () => localStorage.getItem(READER_ROTATION_STORAGE_KEY) === "true",
    );

  const setColorScheme = useCallback((scheme: ReaderColorScheme): void => {
    setColorSchemeState(scheme);
    localStorage.setItem(COLOR_SCHEME_STORAGE_KEY, scheme);
  }, []);

  const setFontScale = useCallback((scale: number): void => {
    if (!READER_FONT_SCALE_OPTIONS.includes(scale)) {
      return;
    }

    setFontScaleState(scale);
    localStorage.setItem(FONT_SCALE_STORAGE_KEY, String(scale));
  }, []);

  const setContentFont = useCallback((font: ReaderContentFont): void => {
    const isValid = READER_CONTENT_FONT_OPTIONS.some(
      (option) => option.value === font,
    );
    if (!isValid) {
      return;
    }

    setContentFontState(font);
    localStorage.setItem(CONTENT_FONT_STORAGE_KEY, font);
  }, []);

  const setReaderRotationEnabled = useCallback((enabled: boolean): void => {
    setReaderRotationEnabledState(enabled);
    localStorage.setItem(READER_ROTATION_STORAGE_KEY, String(enabled));
  }, []);

  const contentFontFamily = READER_CONTENT_FONT_FAMILIES[contentFont];

  return (
    <ReaderColorSchemeContext.Provider
      value={{
        colorScheme,
        setColorScheme,
        fontScale,
        setFontScale,
        contentFont,
        contentFontFamily,
        setContentFont,
        readerRotationEnabled,
        setReaderRotationEnabled,
      }}
    >
      <ThemeProvider theme={themeForScheme[colorScheme]}>
        {children}
      </ThemeProvider>
    </ReaderColorSchemeContext.Provider>
  );
};
