import "dayjs/locale/de";

import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { type ReactElement, useEffect } from "react";
import { BrowserRouter } from "react-router-dom";

import { CssBaseline, ThemeProvider } from "@mui/material";
import { GlobalPollerCollection } from "./components/poller/GlobalPollerCollection";
import { ApiProvider } from "./contexts/ApiProvider";
import { ApiNotificationListener } from "./contexts/notification/ApiNotificationListener";
import { NotificationProvider } from "./contexts/notification/NotificationProvider";
import { UserProfileProvider } from "./contexts/UserProfileContext";
import { UserSettingsProvider } from "./contexts/UserSettingsContext";
import { InitializeMobile } from "./InitializeMobile";
import AppRoutes from "./routes/routes";
import { theme } from "./styles/Theme";

function App(): ReactElement {
  useEffect(() => {
    InitializeMobile();
  }, []);

  return (
    <ApiProvider>
      <UserProfileProvider>
        <UserSettingsProvider>
          <ThemeProvider theme={theme} noSsr>
            <CssBaseline />
            <NotificationProvider>
              <BrowserRouter>
                <ApiNotificationListener />
                <GlobalPollerCollection />
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
