import type { ReactElement } from "react";
import { useRoutes } from "react-router-dom";

import { MainLayout } from "../components/layout/MainLayout";
import BookDetailPage from "../pages/BookDetailPage";
import HomePage from "../pages/HomePage";
import BooksPage from "../pages/LibraryPage";
import SeriesDetailPage from "../pages/SeriesDetailPage";

const AppRoutes = (): ReactElement | null =>
  useRoutes([
    {
      path: "/",
      element: <MainLayout />,
      children: [
        { index: true, element: <HomePage /> },
        { path: "library", element: <BooksPage /> },
        { path: "library/series/:seriesId", element: <SeriesDetailPage /> },
        { path: "library/books/:bookId", element: <BookDetailPage /> },
      ],
    },
  ]);

export default AppRoutes;
