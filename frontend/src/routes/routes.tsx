import type { ReactElement } from "react";
import { useRoutes } from "react-router-dom";

import { MainLayout } from "../components/layout/MainLayout";
import BooksPage from "../pages/BooksPage";
import HomePage from "../pages/HomePage";

const AppRoutes = (): ReactElement | null =>
  useRoutes([
    {
      path: "/",
      element: <MainLayout />,
      children: [
        { index: true, element: <HomePage /> },
        { path: "/books", element: <BooksPage /> },
      ],
    },
  ]);

export default AppRoutes;
