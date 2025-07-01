import type { ReactElement } from "react";
import { useRoutes } from "react-router-dom";

import { MainLayout } from "../components/layout/MainLayout";
import { useUserProfile } from "../contexts/UserProfileContext";
import BookDetailPage from "../pages/book/BookDetailPage";
import CreateBookPage from "../pages/book/CreateBookPage";
import EditBookDetailPage from "../pages/book/EditBookDetailPage";
import ImportBookPage from "../pages/book/ImportBookPage";
import HomePage from "../pages/HomePage";
import BooksPage from "../pages/LibraryPage";
import SearchResultsPage from "../pages/SearchResultsPage";
import EditSeriesDetailPage from "../pages/series/EditSeriesDetailPage";
import SeriesDetailPage from "../pages/series/SeriesDetailPage";
import { UserProfileSelectionPage } from "../pages/UserProfileSelectionPage";

const AppRoutes = (): ReactElement | null => {
  const { profile } = useUserProfile();

  return useRoutes([
    {
      path: "/",
      element: profile === null ? <UserProfileSelectionPage /> : <MainLayout />,
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
        { path: "search", element: <SearchResultsPage /> },
      ],
    },
  ]);
};

export default AppRoutes;
