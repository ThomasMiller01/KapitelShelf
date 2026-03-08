import { ThemeProvider } from "@mui/material";
import {
  createContext,
  type ReactNode,
  useCallback,
  useContext,
  useState,
} from "react";
import { createReaderTheme, ReaderColorScheme } from "../../../styles/Theme";

const STORAGE_KEY = "kapitelshelf.reader.colorscheme";

interface ReaderColorSchemeContextValue {
  colorScheme: ReaderColorScheme;
  setColorScheme: (scheme: ReaderColorScheme) => void;
}

const ReaderColorSchemeContext = createContext<ReaderColorSchemeContextValue>({
  colorScheme: "light",
  setColorScheme: () => undefined,
});

export const useReaderColorScheme = (): ReaderColorSchemeContextValue =>
  useContext(ReaderColorSchemeContext);

const themeForScheme = {
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
    () => (localStorage.getItem(STORAGE_KEY) as ReaderColorScheme) ?? "light",
  );

  const setColorScheme = useCallback((scheme: ReaderColorScheme): void => {
    setColorSchemeState(scheme);
    localStorage.setItem(STORAGE_KEY, scheme);
  }, []);

  return (
    <ReaderColorSchemeContext.Provider value={{ colorScheme, setColorScheme }}>
      <ThemeProvider theme={themeForScheme[colorScheme]}>
        {children}
      </ThemeProvider>
    </ReaderColorSchemeContext.Provider>
  );
};
