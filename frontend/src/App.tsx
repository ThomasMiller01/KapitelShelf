import "dayjs/locale/de";

import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { SnackbarProvider } from "notistack";
import type { ReactElement } from "react";
import { BrowserRouter } from "react-router-dom";

import { ApiNotificationListener } from "./components/base/feedback/ApiNotificationListener";
import { ThemeProvider } from "./contexts/ThemeContext";
import AppRoutes from "./routes/routes";

const queryClient = new QueryClient();

function App(): ReactElement {
  return (
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <SnackbarProvider>
          <ApiNotificationListener />
          <BrowserRouter>
            <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="de">
              <AppRoutes />
            </LocalizationProvider>
          </BrowserRouter>
        </SnackbarProvider>
      </QueryClientProvider>
    </ThemeProvider>
  );
}

export default App;
