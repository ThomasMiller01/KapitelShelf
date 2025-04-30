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
          <AppRoutes />
        </BrowserRouter>
      </SnackbarProvider>
    </ThemeProvider>
  );
}

export default App;
