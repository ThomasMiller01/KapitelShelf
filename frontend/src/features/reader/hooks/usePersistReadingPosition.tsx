import { useCallback, useEffect, useRef } from "react";

import { useApi } from "../../../shared/contexts/ApiProvider";
import { useUserProfile } from "../../../shared/hooks/useUserProfile";

const pendingCleanupSaves = new Map<string, number>();

const buildReadingRequestUrl = (
  basePath: string,
  bookId: string,
  userId: string,
): string => {
  const trimmedBasePath = basePath.endsWith("/")
    ? basePath.slice(0, -1)
    : basePath;

  return new URL(
    `${trimmedBasePath}/books/${encodeURIComponent(bookId)}/reading?userId=${encodeURIComponent(userId)}`,
    window.location.origin,
  ).toString();
};

export const usePersistReadingPosition = (
  bookId: string,
  section: number,
  page: number,
): void => {
  const { basePath, clients } = useApi();
  const { profile } = useUserProfile();
  const didPersistRef = useRef(false);
  const latestReadingPositionRef = useRef({
    basePath,
    bookId,
    currentPage: page,
    currentSection: section,
    userId: profile?.id,
  });

  useEffect(() => {
    latestReadingPositionRef.current = {
      basePath,
      bookId,
      currentPage: page,
      currentSection: section,
      userId: profile?.id,
    };
  }, [basePath, bookId, page, profile?.id, section]);

  const persistCurrentLocation = useCallback(
    async (transport: "api" | "keepalive"): Promise<void> => {
      if (didPersistRef.current) {
        return;
      }

      const {
        basePath: latestBasePath,
        bookId: latestBookId,
        currentPage,
        currentSection,
        userId,
      } = latestReadingPositionRef.current;

      if (userId === undefined) {
        return;
      }

      didPersistRef.current = true;

      const readingLocation = {
        currentPage,
        currentSection,
      };

      if (transport === "keepalive") {
        await fetch(
          buildReadingRequestUrl(latestBasePath, latestBookId, userId),
          {
            body: JSON.stringify(readingLocation),
            headers: {
              "Content-Type": "application/json",
            },
            keepalive: true,
            method: "PUT",
          },
        );
        return;
      }

      await clients.books.booksBookIdReadingPut(
        latestBookId,
        userId,
        readingLocation,
      );
    },
    [clients.books],
  );

  useEffect(() => {
    didPersistRef.current = false;

    const pendingCleanupSave = pendingCleanupSaves.get(bookId);
    if (pendingCleanupSave !== undefined) {
      window.clearTimeout(pendingCleanupSave);
      pendingCleanupSaves.delete(bookId);
    }

    return (): void => {
      const timeoutId = window.setTimeout(() => {
        if (pendingCleanupSaves.get(bookId) !== timeoutId) {
          return;
        }

        pendingCleanupSaves.delete(bookId);
        void persistCurrentLocation("api");
      }, 0);

      pendingCleanupSaves.set(bookId, timeoutId);
    };
  }, [bookId, persistCurrentLocation]);

  useEffect(() => {
    const handlePageHide = (): void => {
      void persistCurrentLocation("keepalive");
    };

    const handlePageShow = (): void => {
      didPersistRef.current = false;
    };

    window.addEventListener("pagehide", handlePageHide);
    window.addEventListener("pageshow", handlePageShow);

    return (): void => {
      window.removeEventListener("pagehide", handlePageHide);
      window.removeEventListener("pageshow", handlePageShow);
    };
  }, [persistCurrentLocation]);
};
