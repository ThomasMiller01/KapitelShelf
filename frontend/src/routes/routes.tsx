import type { ReactElement } from "react";
import { useRoutes } from "react-router-dom";

import { MainLayout } from "../components/layout/MainLayout";
import BookDetailPage from "../pages/book/BookDetailPage";
import CreateBookPage from "../pages/book/CreateBookPage";
import EditBookDetailPage from "../pages/book/EditBookDetailPage";
import ImportBookPage from "../pages/book/ImportBookPage";
import HomePage from "../pages/HomePage";
import BooksPage from "../pages/LibraryPage";
import EditSeriesDetailPage from "../pages/series/EditSeriesDetailPage";
import SeriesDetailPage from "../pages/series/SeriesDetailPage";

const AppRoutes = (): ReactElement | null =>
  useRoutes([
    {
      path: "/",
      element: <MainLayout />,
      children: [
        { index: true, element: <HomePage /> },
        { path: "library", element: <BooksPage /> },
        { path: "library/series/:seriesId", element: <SeriesDetailPage /> },
        {
          path: "library/series/:seriesId/edit",
          element: <EditSeriesDetailPage />,
        },
        { path: "library/books/:bookId", element: <BookDetailPage /> },
        { path: "library/books/:bookId/edit", element: <EditBookDetailPage /> },
        { path: "library/books/create", element: <CreateBookPage /> },
        { path: "library/books/import", element: <ImportBookPage /> },
      ],
    },
  ]);

export default AppRoutes;
