import { Box, Button, Typography } from "@mui/material";
import type { ReactElement } from "react";

type WORDING_TYPES = "creatable" | "normal";
interface NoItemsFoundCardProps {
  itemName: string;
  useLogo?: boolean;
  onCreate?: () => void;
  small?: boolean;
  extraSmall?: boolean;
  wording?: WORDING_TYPES;
}

export const NoItemsFoundCard = ({
  itemName,
  useLogo = false,
  onCreate,
  small = false,
  extraSmall = false,
  wording = "creatable",
}: NoItemsFoundCardProps): ReactElement => (
  <Box
    sx={{
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      height: "100%",
      py: extraSmall ? 2 : 10,
    }}
  >
    {useLogo && (
      <img src="/kapitelshelf.png" alt="KapitelShelf Logo" width={120} />
    )}
    <Typography
      variant={extraSmall ? "body1" : small ? "h6" : "h5"}
      my={small || extraSmall ? 0 : 4}
      textTransform="uppercase"
    >
      {GetWording(wording, itemName)}
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

const GetWording = (wordingType: WORDING_TYPES, itemName: string): string => {
  switch (wordingType) {
    case "creatable":
      return `Looks like you don't have any ${itemName} yet`;
    case "normal":
      return `No ${itemName}`;
    default:
      return GetWording("creatable", itemName);
  }
};
