import { Stack, Typography } from "@mui/material";
import type { SnackbarKey } from "notistack";
import { useSnackbar } from "notistack";
import type { ReactElement } from "react";
import { useCallback, useRef, useState } from "react";

import { DotsProgress } from "../components/base/feedback/DotsProgress";

interface triggerProps {
  operation: string | undefined;
}

interface triggerLoadingProps extends triggerProps {
  delay?: number;
  open?: boolean;
  close?: boolean;
}

interface triggerErrorProps extends triggerProps {
  errorMessage: string;
}

interface ApiNotificationResult {
  triggerLoading: (props: triggerLoadingProps) => void;
  triggerError: (props: triggerErrorProps) => void;
  triggerSuccess: (props: triggerProps) => void;
}

export const useApiNotification = (): ApiNotificationResult => {
  const { enqueueSnackbar, closeSnackbar } = useSnackbar();

  const [loadingNotifId, setLoadingNotifId] = useState<SnackbarKey>();
  const loadingTimeout = useRef<number>(undefined);
  const triggerLoading = useCallback(
    ({ open, close, delay = 0, ...props }: triggerLoadingProps) => {
      if (open) {
        // if we already have a timer or a visible toast, do nothing
        if (loadingTimeout.current || loadingNotifId !== undefined) {
          return;
        }

        loadingTimeout.current = setTimeout(() => {
          const id = enqueueSnackbar(<LoadingMessage {...props} />, {
            variant: "info",
            persist: true,
          });
          setLoadingNotifId(id);
          loadingTimeout.current = undefined;
        }, delay);
      } else if (close) {
        // cancel any pending toast
        if (loadingTimeout.current) {
          clearTimeout(loadingTimeout.current);
          loadingTimeout.current = undefined;
        }
        // dismiss the visible toast
        if (loadingNotifId !== undefined) {
          closeSnackbar(loadingNotifId);
          setLoadingNotifId(undefined);
        }
      }
    },
    [enqueueSnackbar, closeSnackbar, loadingNotifId]
  );

  const triggerError = (props: triggerErrorProps): void => {
    enqueueSnackbar(<ErrorMessage {...props} />, {
      variant: "error",
      autoHideDuration: 8000,
    });
  };

  const triggerSuccess = (props: triggerProps): void => {
    enqueueSnackbar(<SuccessMessage {...props} />, {
      variant: "success",
    });
  };

  return {
    triggerLoading,
    triggerError,
    triggerSuccess,
  };
};

const LoadingMessage = ({ operation }: triggerProps): ReactElement => (
  <Stack direction="row" spacing={0.5} alignItems="center">
    <Typography>{operation}</Typography>
    <DotsProgress small initialDots={1} />
  </Stack>
);

const ErrorMessage = ({
  operation,
  errorMessage,
}: triggerErrorProps): ReactElement => (
  <Stack direction="row" spacing={0.5} alignItems="center">
    <Typography>{operation} failed:</Typography>
    <Typography>{errorMessage}</Typography>
  </Stack>
);

const SuccessMessage = ({ operation }: triggerProps): ReactElement => (
  <Stack direction="row" spacing={0.5} alignItems="center">
    <Typography>{operation} succeeded</Typography>
  </Stack>
);
