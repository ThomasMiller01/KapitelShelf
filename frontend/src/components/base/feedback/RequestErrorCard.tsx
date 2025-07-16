import { Box, Button, Typography } from "@mui/material";
import type { ReactElement } from "react";

interface RequestErrorCardProps {
  itemName: string;
  onRetry?: () => void;
  small?: boolean;
}

export const RequestErrorCard = ({
  itemName,
  onRetry,
  small = false,
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
      Failed to load {itemName}.
    </Typography>
    <Typography variant="body1" color="text.secondary" mb={small ? 2 : 3}>
      Please check your internet connection or try again later.
    </Typography>
    {onRetry && (
      <Button variant="outlined" onClick={onRetry}>
        Retry
      </Button>
    )}
  </Box>
);
