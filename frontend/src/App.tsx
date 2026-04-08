import "dayjs/locale/de";

import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { type ReactElement, useEffect } from "react";
import { BrowserRouter } from "react-router-dom";

import { CssBaseline, ThemeProvider } from "@mui/material";
import { InitializeMobile } from "./InitializeMobile";
import AppRoutes from "./routes";
import { ApiProvider } from "./shared/contexts/ApiProvider";
import { ApiNotificationListener } from "./shared/contexts/notification/ApiNotificationListener";
import { NotificationProvider } from "./shared/contexts/notification/NotificationProvider";
import { UserProfileProvider } from "./shared/contexts/UserProfileContext";
import { UserSettingsProvider } from "./shared/contexts/UserSettingsContext";
import { KapitelShelfGlobalStyles } from "./styles/GlobalStyles";
import { theme } from "./styles/Theme";

function App(): ReactElement {
  useEffect(() => {
    InitializeMobile();
  }, []);

  return (
    <ApiProvider>
      <UserProfileProvider>
        <UserSettingsProvider>
          <ThemeProvider
            theme={theme}
            noSsr
            modeStorageKey="kapitelshelf.colorscheme"
          >
            <CssBaseline />
            <KapitelShelfGlobalStyles />
            <NotificationProvider>
              <BrowserRouter>
                <ApiNotificationListener />
                <LocalizationProvider
                  dateAdapter={AdapterDayjs}
                  adapterLocale="de"
                >
                  <AppRoutes />
                </LocalizationProvider>
              </BrowserRouter>
            </NotificationProvider>
          </ThemeProvider>
        </UserSettingsProvider>
      </UserProfileProvider>
    </ApiProvider>
  );
}

export default App;
