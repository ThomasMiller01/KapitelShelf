import type { Theme } from "@mui/material";
import { CssBaseline, ThemeProvider as MUIThemeProvider } from "@mui/material";
import type { FC, ReactNode } from "react";
import { createContext, useContext, useMemo, useState } from "react";

import { getTheme } from "../styles/Theme";

type ThemeMode = "light" | "dark";

const ThemeToggleContext = createContext<() => void>(() => {});
const ThemeModeContext = createContext<ThemeMode>("light");

export const useToggleTheme = (): (() => void) =>
  useContext(ThemeToggleContext);
export const useThemeMode = (): ThemeMode => useContext(ThemeModeContext);

export const ThemeProvider: FC<{ children: ReactNode }> = ({ children }) => {
  const [mode, setMode] = useState<ThemeMode>("dark");

  const toggleTheme = (): void => {
    setMode((prev) => (prev === "light" ? "dark" : "light"));
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
