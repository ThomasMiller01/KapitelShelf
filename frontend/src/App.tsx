import "dayjs/locale/de";

import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { BrowserRouter } from "react-router-dom";

import { ApiNotificationListener } from "./contexts/notification/ApiNotificationListener";
import { NotificationProvider } from "./contexts/notification/NotificationProvider";
import { ThemeProvider } from "./contexts/ThemeContext";
import { UserProfileProvider } from "./contexts/UserProfileContext";
import AppRoutes from "./routes/routes";

const queryClient = new QueryClient();

function App(): ReactElement {
  return (
    <UserProfileProvider>
      <ThemeProvider>
        <QueryClientProvider client={queryClient}>
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
        </QueryClientProvider>
      </ThemeProvider>
    </UserProfileProvider>
  );
}

export default App;
