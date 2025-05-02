import { Link, Stack, Typography } from "@mui/material";
import { useSnackbar } from "notistack";
import { useCallback } from "react";

export const useNotImplemented = (): ((issue: number) => void) => {
  const { enqueueSnackbar } = useSnackbar();

  const trigger = useCallback(
    (issue: number) => {
      enqueueSnackbar(
        <Stack direction="row" spacing={1} alignItems="center">
          <Typography>Not Implemented</Typography>
          <Link
            href={`https://github.com/ThomasMiller01/KapitelShelf/issues/${issue}`}
            fontSize="1rem"
            target="_blank"
            rel="noopener"
          >
            [Issue #{issue}]
          </Link>
        </Stack>,
        {
          variant: "info",
        }
      );
    },
    [enqueueSnackbar]
  );

  return trigger;
};
