import type {
  MutationState,
  NotifyEventType,
  QueryState,
} from "@tanstack/react-query";
import { useQueryClient } from "@tanstack/react-query";
import type { AxiosError } from "axios";
import { useCallback, useEffect } from "react";

import { useNotification } from "../../../hooks/useNotification";

interface NotifyMetadata {
  enabled: boolean; // default: false
  operation: string;
  showLoading?: boolean; // default: true
  showError?: boolean; // default: true
  showSuccess?: boolean; // default: false
}

interface ErrorData {
  error: string;
}

export const ApiNotificationListener = (): null => {
  const client = useQueryClient();
  const { triggerLoading, triggerError, triggerSuccess } = useNotification();

  const handleNotifications = useCallback(
    (
      type: NotifyEventType,
      meta: Record<string, unknown> | undefined,
      state: QueryState | MutationState | undefined
    ): void => {
      // map meta.notify to NotifyMetadata type
      const notify = meta?.notify as NotifyMetadata | undefined;

      if (notify?.enabled !== true) {
        // notifications disabled
        // enable with: `meta: { notify: { enabled: true, operation: "my-operation" } },` in query options
        return;
      }

      // Only handle true "updated" events, not observerAdded/added/etc.
      if (type !== "updated") {
        return;
      }

      if (state?.status === "pending" && notify.showLoading !== false) {
        // Fire on loading after a 1 second delay
        triggerLoading({
          operation: notify.operation,
          delay: 600,
          open: true,
        });
        return;
      } else if (state?.status === "error" && notify.showError !== false) {
        const error = state.error as AxiosError<unknown>;

        const raw = error.response?.data;
        let errorData: ErrorData | undefined;
        if (raw && typeof raw === "object" && "error" in raw) {
          errorData = raw as ErrorData;
        }

        // Fire on error
        triggerError({
          operation: notify.operation,
          errorMessage: errorData?.error ?? state.error?.message ?? "",
        });
      } else if (state?.status === "success" && notify.showSuccess === true) {
        // Fire on success
        triggerSuccess({ operation: notify.operation });
      }

      // close loading notification
      triggerLoading({ operation: undefined, close: true });
    },
    [triggerLoading, triggerError, triggerSuccess]
  );

  useEffect(() => {
    // Subscribe to all query cache updates
    const unsubscribeQueries = client.getQueryCache().subscribe((event) => {
      const { query } = event;

      handleNotifications(event.type, query.meta, query.state);
    });

    // Subscribe to all mutation cache updates
    const unsubscribeMutations = client
      .getMutationCache()
      .subscribe((event) => {
        const { mutation } = event;

        handleNotifications(event.type, mutation?.meta, mutation?.state);
      });

    return (): void => {
      unsubscribeQueries();
      unsubscribeMutations();
    };
  }, [client, handleNotifications]);

  return null;
};
