import { CssBaseline, ThemeProvider } from "@mui/material";
import type { ReactElement } from "react";
import { BrowserRouter } from "react-router-dom";

import AppRoutes from "./routes/routes";
import GetTheme from "./styles/theme";

function App(): ReactElement {
  return (
    <ThemeProvider theme={GetTheme()}>
      <CssBaseline />
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;
