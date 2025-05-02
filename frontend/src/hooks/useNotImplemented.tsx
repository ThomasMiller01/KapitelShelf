import { Link, Stack, Typography } from "@mui/material";
import { useSnackbar } from "notistack";
import { useCallback } from "react";

declare const __APP_VERSION__: string;

interface useNotImplementedProps {
  issueNumber: number;
}

export const useNotImplemented = ({
  issueNumber,
}: useNotImplementedProps): (() => void) => {
  const { enqueueSnackbar } = useSnackbar();

  const trigger = useCallback(() => {
    enqueueSnackbar(
      <Stack direction="row" spacing={1} alignItems="center">
        <Typography>Not Implemented</Typography>
        <Link
          href={`https://github.com/ThomasMiller01/KapitelShelf/issues/${issueNumber}`}
          fontSize="1rem"
          target="_blank"
          rel="noopener"
        >
          [Issue #{issueNumber}]
        </Link>
      </Stack>,
      {
        variant: "info",
      }
    );
  }, [enqueueSnackbar, issueNumber]);

  return trigger;
};
