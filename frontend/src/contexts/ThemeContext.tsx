import DarkModeIcon from "@mui/icons-material/DarkMode";
import LightModeIcon from "@mui/icons-material/LightMode";
import type { Theme } from "@mui/material";
import {
  CssBaseline,
  IconButton,
  ThemeProvider as MUIThemeProvider,
} from "@mui/material";
import type { FC, ReactElement, ReactNode } from "react";
import { createContext, useContext, useMemo, useState } from "react";

import { getTheme } from "../styles/Theme";

type ThemeMode = "light" | "dark";

const THEME_LOCAL_STORAGE_KEY = "kapitelshelf.theme";

const ThemeToggleContext = createContext<() => void>(() => {});
const ThemeModeContext = createContext<ThemeMode>("light");

export const useToggleTheme = (): (() => void) =>
  useContext(ThemeToggleContext);
export const useThemeMode = (): ThemeMode => useContext(ThemeModeContext);

export const ThemeProvider: FC<{ children: ReactNode }> = ({ children }) => {
  const getDefaultMode = (): ThemeMode => {
    const stored = localStorage.getItem(THEME_LOCAL_STORAGE_KEY);
    if (stored === "light" || stored === "dark") {
      return stored;
    }

    const prefersDark = window.matchMedia(
      "(prefers-color-scheme: dark)"
    ).matches;
    return prefersDark ? "dark" : "light";
  };

  const [mode, setMode] = useState<ThemeMode>(getDefaultMode);

  const toggleTheme = (): void => {
    setMode((prev) => {
      const next = prev === "light" ? "dark" : "light";
      localStorage.setItem(THEME_LOCAL_STORAGE_KEY, next);
      return next;
    });
  };

  const theme: Theme = useMemo(() => getTheme(mode), [mode]);

  return (
    <ThemeModeContext.Provider value={mode}>
      <ThemeToggleContext.Provider value={toggleTheme}>
        <MUIThemeProvider theme={theme}>
          <CssBaseline />
          {children}
        </MUIThemeProvider>
      </ThemeToggleContext.Provider>
    </ThemeModeContext.Provider>
  );
};

export const ThemeToggle = (): ReactElement => {
  const mode = useThemeMode();
  const toggle = useToggleTheme();

  return (
    <IconButton onClick={toggle}>
      {mode === "dark" ? <LightModeIcon /> : <DarkModeIcon />}
    </IconButton>
  );
};
