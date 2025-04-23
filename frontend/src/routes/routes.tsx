import type { ReactElement } from "react";
import { useRoutes } from "react-router-dom";

const AppRoutes = (): ReactElement | null =>
  useRoutes([
    {
      path: "/",
      element: <h1>Home :)</h1>,
    },
  ]);

export default AppRoutes;
