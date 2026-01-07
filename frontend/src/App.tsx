import "dayjs/locale/de";

import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { type ReactElement, useEffect } from "react";
import { BrowserRouter } from "react-router-dom";

import { GlobalPollerCollection } from "./components/poller/GlobalPollerCollection";
import { ApiProvider } from "./contexts/ApiProvider";
import { ApiNotificationListener } from "./contexts/notification/ApiNotificationListener";
import { NotificationProvider } from "./contexts/notification/NotificationProvider";
import { ThemeProvider } from "./contexts/ThemeContext";
import { UserProfileProvider } from "./contexts/UserProfileContext";
import { UserSettingsProvider } from "./contexts/UserSettingsContext";
import { InitializeMobile } from "./InitializeMobile";
import AppRoutes from "./routes/routes";

const queryClient = new QueryClient();

function App(): ReactElement {
  useEffect(() => {
    InitializeMobile();
  }, []);

  return (
    <ApiProvider>
      <UserProfileProvider>
        <UserSettingsProvider>
          <ThemeProvider>
            <QueryClientProvider client={queryClient}>
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
            </QueryClientProvider>
          </ThemeProvider>
        </UserSettingsProvider>
      </UserProfileProvider>
    </ApiProvider>
  );
}

export default App;
