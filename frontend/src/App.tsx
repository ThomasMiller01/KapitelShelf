import "dayjs/locale/de";

import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { SnackbarProvider } from "notistack";
import type { ReactElement } from "react";
import { BrowserRouter } from "react-router-dom";

import { ThemeProvider } from "./contexts/ThemeContext";
import AppRoutes from "./routes/routes";

function App(): ReactElement {
  return (
    <ThemeProvider>
      <SnackbarProvider>
        <BrowserRouter>
          <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="de">
            <AppRoutes />
          </LocalizationProvider>
        </BrowserRouter>
      </SnackbarProvider>
    </ThemeProvider>
  );
}

export default App;
