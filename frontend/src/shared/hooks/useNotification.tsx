import { Button, Link, Stack, Typography } from "@mui/material";
import Divider from "@mui/material/Divider";
import type { SnackbarKey } from "notistack";
import { useSnackbar } from "notistack";
import type { ReactElement } from "react";
import { useCallback, useRef, useState } from "react";

import { DotsProgress } from "../components/base/feedback/DotsProgress";
import { useMobile } from "./useMobile";

interface triggerApiProps {
  operation: string | undefined;
}

interface triggerLoadingProps extends triggerApiProps {
  delay?: number;
  open?: boolean;
  close?: boolean;
}

interface triggerErrorProps extends triggerApiProps {
  errorMessage: string;
}

interface triggerNavigateProps extends triggerApiProps {
  itemName: string;
  url: string;
}

interface NotificationResult {
  triggerLoading: (props: triggerLoadingProps) => void;
  triggerError: (props: triggerErrorProps) => void;
  triggerSuccess: (props: triggerApiProps) => void;
  triggerNavigate: (props: triggerNavigateProps) => void;
}

export const useNotification = (): NotificationResult => {
  const { enqueueSnackbar, closeSnackbar } = useSnackbar();

  const [loadingNotifId, setLoadingNotifId] = useState<SnackbarKey>();
  const loadingTimeout = useRef<NodeJS.Timeout>(undefined);
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

  const triggerSuccess = (props: triggerApiProps): void => {
    enqueueSnackbar(<SuccessMessage {...props} />, {
      variant: "success",
    });
  };

  const triggerNavigate = (props: triggerNavigateProps): void => {
    enqueueSnackbar(<NavigateMessage {...props} />, {
      variant: "info",
      autoHideDuration: 8000,
      action: (snackbarId) => (
        <Button
          component={Link}
          variant="text"
          href={props.url ?? undefined}
          onClick={() => {
            closeSnackbar(snackbarId);
          }}
        >
          View
        </Button>
      ),
    });
  };

  return {
    triggerLoading,
    triggerError,
    triggerSuccess,
    triggerNavigate,
  };
};

const LoadingMessage = ({ operation }: triggerLoadingProps): ReactElement => (
  <Stack direction="row" spacing={0.5} alignItems="center">
    <Typography>{operation}</Typography>
    <DotsProgress small initialDots={1} />
  </Stack>
);

const ErrorMessage = ({
  operation,
  errorMessage,
}: triggerErrorProps): ReactElement => {
  const { isMobile } = useMobile();
  return (
    <Stack
      direction={{ xs: "column", md: "row" }}
      spacing={{ xs: 0, md: 0.5 }}
      alignItems={{ xs: "left", md: "center" }}
      divider={
        isMobile ? (
          <Divider
            sx={{
              borderColor: "white",
              opacity: "0.85",
              mb: "6px !important",
            }}
          />
        ) : undefined
      }
    >
      <Typography>{operation} failed:</Typography>
      <Typography fontSize={isMobile ? "0.85rem" : "1rem"}>
        {errorMessage}
      </Typography>
    </Stack>
  );
};

const SuccessMessage = ({ operation }: triggerApiProps): ReactElement => (
  <Stack direction="row" spacing={0.5} alignItems="center">
    <Typography>{operation} succeeded</Typography>
  </Stack>
);

const NavigateMessage = ({
  operation,
  itemName,
}: triggerNavigateProps): ReactElement => (
  <Stack direction="row" spacing={0.5} alignItems="center">
    <Typography>
      {operation}: "{itemName}".
    </Typography>
  </Stack>
);
