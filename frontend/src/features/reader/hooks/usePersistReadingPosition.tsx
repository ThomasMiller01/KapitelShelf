import { useSnackbar } from "notistack";
import { useCallback, useEffect, useRef } from "react";

import { useApi } from "../../../shared/contexts/ApiProvider";
import { useUserProfile } from "../../../shared/hooks/useUserProfile";
import type { ReaderLocation } from "../utils/readerPosition";

export const usePersistReadingPosition = (bookId: string, position?: ReaderLocation): void => {
  const { clients: { books } } = useApi();
  const { profile } = useUserProfile();
  const { enqueueSnackbar } = useSnackbar();
  const userId = profile?.id;
  const pending = useRef<ReaderLocation>(undefined);
  const queue = useRef(Promise.resolve());

  const save = useCallback((keepalive = false): void => {
    const target = pending.current;
    if (!target || !userId) {
      return;
    }
    pending.current = undefined;
    const body = { currentSection: target.section, currentPage: target.page };
    const options = keepalive ? { adapter: "fetch", fetchOptions: { keepalive: true } } : undefined;
    queue.current = queue.current
      .then(async () => { await books.booksBookIdReadingPut(bookId, userId, body, options); })
      .catch(() => {
        enqueueSnackbar("Could not save the reading position.", { variant: "warning", preventDuplicate: true });
      });
  }, [bookId, books, enqueueSnackbar, userId]);

  useEffect(() => {
    pending.current = position;
    if (!position) {
      return;
    }
    const timer = window.setTimeout(save, 500);
    return (): void => window.clearTimeout(timer);
  }, [position, save]);

  useEffect(() => {
    const leave = (): void => save(true);
    window.addEventListener("pagehide", leave);
    return (): void => {
      window.removeEventListener("pagehide", leave);
      leave();
    };
  }, [save]);
};
