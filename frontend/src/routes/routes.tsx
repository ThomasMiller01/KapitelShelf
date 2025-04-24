import type { ReactElement } from "react";
import { useRoutes } from "react-router-dom";

import { MainLayout } from "../components/layout/MainLayout";
import HomePage from "../pages/HomePage";
import BooksPage from "../pages/LibraryPage";

const AppRoutes = (): ReactElement | null =>
  useRoutes([
    {
      path: "/",
      element: <MainLayout />,
      children: [
        { index: true, element: <HomePage /> },
        { path: "/library", element: <BooksPage /> },
      ],
    },
  ]);

export default AppRoutes;
