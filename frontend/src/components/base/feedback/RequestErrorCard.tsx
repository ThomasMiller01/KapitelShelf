import { Box, Button, Stack, Typography } from "@mui/material";
import type { ReactElement } from "react";

interface RequestErrorCardProps {
  itemName: string;
  actionText?: string;
  subtitle?: string;
  onRetry?: () => void;
  small?: boolean;
  secondAction?: (() => void) | null;
  secondActionText?: string | null;
  secondActionIcon?: ReactElement | null;
}

export const RequestErrorCard = ({
  itemName,
  actionText = "load",
  subtitle = "Please check your internet connection or try again later.",
  onRetry,
  small = false,
  secondAction = null,
  secondActionText = null,
  secondActionIcon = null,
}: RequestErrorCardProps): ReactElement => (
  <Box
    sx={{
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      height: "100%",
      py: small ? 1 : 10,
    }}
  >
    <Typography variant="h5" color="error" gutterBottom>
      Failed to {actionText} {itemName}.
    </Typography>
    <Typography variant="body1" color="text.secondary" mb={small ? 2 : 3}>
      {subtitle}
    </Typography>
    <Stack direction="row" spacing={2}>
      {onRetry && (
        <Button variant="outlined" onClick={onRetry}>
          Retry
        </Button>
      )}
      {secondAction && secondActionText && (
        <Button
          variant="contained"
          onClick={secondAction}
          color="secondary"
          startIcon={secondActionIcon}
        >
          {secondActionText}
        </Button>
      )}
    </Stack>
  </Box>
);
