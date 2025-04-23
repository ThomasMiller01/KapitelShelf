import type { ReactElement } from "react";
import { useRoutes } from "react-router-dom";

import HomePage from "../pages/Home";

const AppRoutes = (): ReactElement | null =>
  useRoutes([
    {
      path: "/",
      element: <HomePage />,
    },
  ]);

export default AppRoutes;
