import { Box, Button, Typography } from "@mui/material";
import type { ReactElement } from "react";
interface NoItemsFoundCardProps {
  itemName: string;
  useLogo?: boolean;
  onCreate?: () => void;
}

export const NoItemsFoundCard = ({
  itemName,
  useLogo = false,
  onCreate,
}: NoItemsFoundCardProps): ReactElement => (
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
    {useLogo && (
      <img src="/kapitelshelf.png" alt="KapitelShelf Logo" width={120} />
    )}
    <Typography variant="h5" my={4} textTransform="uppercase">
      Looks like you don't have any {itemName} yet
    </Typography>
    {onCreate && (
      <Typography
        variant="body1"
        mb={2}
        textTransform="uppercase"
        color="text.secondary"
      >
        Try adding your first {itemName} below!
      </Typography>
    )}
    {onCreate && (
      <Button variant="outlined" onClick={onCreate}>
        Create first {itemName}
      </Button>
    )}
  </Box>
);
