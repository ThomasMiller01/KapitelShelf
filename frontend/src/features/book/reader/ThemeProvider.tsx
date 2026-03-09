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
export const READER_FONT_SCALE_OPTIONS = [
  0.8, 0.9, 1, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6,
];
const DEFAULT_FONT_SCALE = 1;

interface ReaderColorSchemeContextValue {
  colorScheme: ReaderColorScheme;
  setColorScheme: (scheme: ReaderColorScheme) => void;
  fontScale: number;
  setFontScale: (scale: number) => void;
}

const ReaderColorSchemeContext = createContext<ReaderColorSchemeContextValue>({
  colorScheme: "light",
  setColorScheme: () => undefined,
  fontScale: DEFAULT_FONT_SCALE,
  setFontScale: () => undefined,
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

  return (
    <ReaderColorSchemeContext.Provider
      value={{ colorScheme, setColorScheme, fontScale, setFontScale }}
    >
      <ThemeProvider theme={themeForScheme[colorScheme]}>
        {children}
      </ThemeProvider>
    </ReaderColorSchemeContext.Provider>
  );
};
