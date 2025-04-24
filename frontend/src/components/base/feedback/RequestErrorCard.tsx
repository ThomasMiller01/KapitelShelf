import { Box, Button, Typography } from "@mui/material";
import type { ReactElement } from "react";

interface ErrorBooksProps {
  onRetry?: () => void;
}

export const RequestErrorCard = ({
  onRetry,
}: ErrorBooksProps): ReactElement => (
  <Box
    sx={{
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      height: "100%",
      py: 10,
    }}
  >
    <Typography variant="h5" color="error" gutterBottom>
      Failed to load books.
    </Typography>
    <Typography variant="body1" color="text.secondary" mb={3}>
      Please check your internet connection or try again later.
    </Typography>
    {onRetry && (
      <Button variant="outlined" onClick={onRetry}>
        Retry
      </Button>
    )}
  </Box>
);
