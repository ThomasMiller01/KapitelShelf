import type { ReactElement } from "react";
import { useRoutes } from "react-router-dom";

import { MainLayout } from "../components/layout/MainLayout";
import { SettingsLayout } from "../components/layout/SettingsLayout";
import { useSessionStart } from "../hooks/apihooks/useSessionStart";
import { useUserProfile } from "../hooks/useUserProfile";
import BookDetailPage from "../pages/book/BookDetailPage";
import CreateBookPage from "../pages/book/CreateBookPage";
import EditBookDetailPage from "../pages/book/EditBookDetailPage";
import ImportBookFromASINPage from "../pages/book/ImportBookFromASINPage";
import ImportBookPage from "../pages/book/ImportBookPage";
import { ConfigureMobileBackendUrlPage } from "../pages/ConfigureMoibleBackendUrlPage";
import HomePage from "../pages/HomePage";
import BooksPage from "../pages/LibraryPage";
import NotificationDetailPage from "../pages/notifications/NotificationDetailPage";
import { NotificationsPage } from "../pages/notifications/NotificationsPage";
import SearchResultsPage from "../pages/SearchResultsPage";
import EditSeriesDetailPage from "../pages/series/EditSeriesDetailPage";
import SeriesDetailPage from "../pages/series/SeriesDetailPage";
import { CloudStoragesPage } from "../pages/settings/CloudStoragesPage";
import { ManageLibraryPage } from "../pages/settings/ManageLibrary";
import { SettingsPage } from "../pages/settings/SettingsPage";
import { TasksPage } from "../pages/settings/TasksPage";
import { CreateProfilePage } from "../pages/user/CreateProfilePage";
import { EditProfilePage } from "../pages/user/EditProfilePage";
import { SelectProfilePage } from "../pages/user/SelectProfilePage";
import { ViewProfilePage } from "../pages/user/ViewProfilePage";
import { WatchlistPage } from "../pages/WatchlistPage";
import {
  IsMobileApiBaseUrlConfigured,
  IsMobileApp,
} from "../utils/MobileUtils";

const AppRoutes = (): ReactElement | null => {
  const { profile } = useUserProfile();
  useSessionStart();

  return useRoutes(
    IsMobileApp() && !IsMobileApiBaseUrlConfigured()
      ? // require mobile app to configure backend URL first, doesnt affect web
        [{ path: "*", element: <ConfigureMobileBackendUrlPage /> }]
      : // then show normal routes
        [
          {
            path: "/",
            element: profile === null ? <SelectProfilePage /> : <MainLayout />,
            children: [
              { index: true, element: <HomePage /> },
              { path: "profile", element: <ViewProfilePage /> },
              { path: "profile/edit", element: <EditProfilePage /> },
              { path: "library", element: <BooksPage /> },
              {
                path: "library/series/:seriesId",
                element: <SeriesDetailPage />,
              },
              {
                path: "library/series/:seriesId/edit",
                element: <EditSeriesDetailPage />,
              },
              { path: "library/books/:bookId", element: <BookDetailPage /> },
              {
                path: "library/books/:bookId/edit",
                element: <EditBookDetailPage />,
              },
              { path: "library/books/create", element: <CreateBookPage /> },
              { path: "library/books/import", element: <ImportBookPage /> },
              {
                path: "library/books/import-from-asin",
                element: <ImportBookFromASINPage />,
              },
              { path: "search", element: <SearchResultsPage /> },
              { path: "watchlist", element: <WatchlistPage /> },
              { path: "notifications", element: <NotificationsPage /> },
              {
                path: "notifications/:notificationId",
                element: <NotificationDetailPage />,
              },
              {
                path: "settings",
                element: <SettingsLayout />,
                children: [
                  {
                    index: true,
                    element: <SettingsPage />,
                  },
                  {
                    path: "tasks",
                    element: <TasksPage />,
                  },
                  {
                    path: "cloudstorages",
                    element: <CloudStoragesPage />,
                  },
                  {
                    path: "manage-library",
                    element: <ManageLibraryPage />,
                  },
                ],
              },
            ],
          },
          {
            path: "/create-user-profile",
            element: <CreateProfilePage />,
          },
        ]
  );
};

export default AppRoutes;
